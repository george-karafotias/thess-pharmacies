using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace ThessPharmacies.Parser;

public sealed class FsthPdfParser
{
    private static readonly Regex PhoneRegex =
        new(@"\b2\d{9}\b", RegexOptions.Compiled);

    private static readonly Regex DateRegex =
        new(
            @"(\d{1,2})\s+([Α-Ωα-ω]+)\s+(\d{4})",
            RegexOptions.Compiled);

    private const double RowTolerance = 2.5;

    private static readonly string[] DaytimeSections =
    {
        "Διημερεύοντα Φαρμακεία"
    };

    private static readonly string[] OvernightSections =
    {
        "Διανυκτερεύοντα Φαρμακεία"
    };

    private static readonly string[] AfterMidnightSections =
    {
        "Μεταμεσονύκτια Φαρμακεία"
    };

    public FsthParseResult Parse(string pdfPath)
    {
        if (string.IsNullOrWhiteSpace(pdfPath))
            throw new ArgumentException(
                "PDF path is required.",
                nameof(pdfPath));

        if (!File.Exists(pdfPath))
            throw new FileNotFoundException(
                "PDF file was not found.",
                pdfPath);

        using var document = PdfDocument.Open(pdfPath);

        var lines = new List<PdfLine>();

        foreach (var page in document.GetPages())
        {
            lines.AddRange(ExtractLines(page));
        }

        return ParseLines(lines);
    }

    private static List<PdfLine> ExtractLines(Page page)
    {
        var words = page.GetWords()
            .Select(x => new PdfWord
            {
                Text = x.Text,
                X = x.BoundingBox.Left,
                Y = x.BoundingBox.Top
            })
            .ToList();

        var lines = new List<PdfLine>();

        foreach (var word in words
                     .OrderByDescending(x => x.Y)
                     .ThenBy(x => x.X))
        {
            var line = lines.FirstOrDefault(
                x => Math.Abs(x.Y - word.Y) <= RowTolerance);

            if (line == null)
            {
                line = new PdfLine
                {
                    Y = word.Y
                };

                lines.Add(line);
            }

            line.Words.Add(word);
        }

        foreach (var line in lines)
        {
            line.Words = line.Words
                .OrderBy(x => x.X)
                .ToList();

            line.Text = string.Join(
                " ",
                line.Words.Select(x => x.Text));
        }

        return lines
            .OrderByDescending(x => x.Y)
            .ToList();
    }

    private static FsthParseResult ParseLines(
        List<PdfLine> lines)
    {
        var result = new FsthParseResult
        {
            DutyDate = ExtractDate(lines)
        };

        DutyType? currentDutyType = null;

        PharmacyRecordBuilder? currentRecord = null;

        foreach (var line in lines)
        {
            var text = line.Text.Trim();

            if (string.IsNullOrWhiteSpace(text))
                continue;

            if (TryGetDutyType(text, out var sectionType))
            {
                FlushRecord(
                    currentRecord,
                    currentDutyType,
                    result);

                currentRecord = null;
                currentDutyType = sectionType;

                continue;
            }

            if (currentDutyType == null)
                continue;

            if (IsColumnHeader(text))
                continue;

            if (IsExplanatoryLine(text))
                continue;

            if (LooksLikeNewRecord(line))
            {
                FlushRecord(
                    currentRecord,
                    currentDutyType,
                    result);

                currentRecord =
                    new PharmacyRecordBuilder(line);

                continue;
            }

            if (currentRecord != null)
            {
                currentRecord.AddContinuation(line);
            }
        }

        FlushRecord(
            currentRecord,
            currentDutyType,
            result);

        Validate(result);

        return result;
    }

    private static bool TryGetDutyType(
        string text,
        out DutyType dutyType)
    {
        foreach (var section in DaytimeSections)
        {
            if (text.Contains(
                    section,
                    StringComparison.OrdinalIgnoreCase))
            {
                dutyType = DutyType.Daytime;
                return true;
            }
        }

        foreach (var section in OvernightSections)
        {
            if (text.Contains(
                    section,
                    StringComparison.OrdinalIgnoreCase))
            {
                dutyType = DutyType.Overnight;
                return true;
            }
        }

        foreach (var section in AfterMidnightSections)
        {
            if (text.Contains(
                    section,
                    StringComparison.OrdinalIgnoreCase))
            {
                dutyType = DutyType.AfterMidnight;
                return true;
            }
        }

        dutyType = default;
        return false;
    }

    private static bool IsColumnHeader(string text)
    {
        return text.Contains(
                   "ΠΕΡΙΟΧΗ",
                   StringComparison.OrdinalIgnoreCase)
            || text.Contains(
                   "ΦΑΡΜΑΚΕΙΟ",
                   StringComparison.OrdinalIgnoreCase)
            || text.Contains(
                   "ΔΙΕΥΘΥΝΣΗ",
                   StringComparison.OrdinalIgnoreCase)
            || text.Contains(
                   "ΤΗΛΕΦΩΝΟ",
                   StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsExplanatoryLine(string text)
    {
        return text.Equals(
            "Τρίτη, Πέμπτη & Παρασκευή",
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksLikeNewRecord(PdfLine line)
    {
        if (line.Words.Count == 0)
            return false;

        var text = line.Text;

        // A phone number is a strong indicator that
        // the line belongs to a new pharmacy record.
        if (PhoneRegex.IsMatch(text))
            return true;

        // The first column contains the area.
        // Area text starts near the left side of the PDF.
        var firstWord = line.Words[0];

        return firstWord.X < 100;
    }

    private static DateOnly ExtractDate(
        List<PdfLine> lines)
    {
        foreach (var line in lines.Take(20))
        {
            var match = DateRegex.Match(line.Text);

            if (!match.Success)
                continue;

            if (!int.TryParse(
                    match.Groups[1].Value,
                    out var day))
            {
                continue;
            }

            var monthText =
                match.Groups[2].Value;

            if (!int.TryParse(
                    match.Groups[3].Value,
                    out var year))
            {
                continue;
            }

            var month = GreekMonth(monthText);

            if (month == 0)
                continue;

            return new DateOnly(
                year,
                month,
                day);
        }

        throw new FormatException(
            "Could not determine the duty date from the PDF.");
    }

    private static int GreekMonth(string value)
    {
        var month = value.Trim().ToLowerInvariant();

        return month switch
        {
            "ιαν" or "ιανουάριος" or "ιανουαρίου" => 1,
            "φεβ" or "φεβρουάριος" or "φεβρουαρίου" => 2,
            "μαρ" or "μάρ" or "μάρτιος" or "μαρτίου" => 3,
            "απρ" or "απρίλιος" or "απριλίου" => 4,
            "μαϊ" or "μαι" or "μάιος" or "μαΐου" => 5,
            "ιουν" or "ιούν" or "ιούνιος" or "ιουνίου" => 6,
            "ιουλ" or "ιούλ" or "ιούλιος" or "ιουλίου" => 7,
            "αυγ" or "αύγ" or "αύγουστος" or "αυγούστου" => 8,
            "σεπ" or "σεπτ" or "σεπτέμβριος" or "σεπτεμβρίου" => 9,
            "οκτ" or "οκτώβριος" or "οκτωβρίου" => 10,
            "νοε" or "νοέ" or "νοέμβριος" or "νοεμβρίου" => 11,
            "δεκ" or "δεκέμβριος" or "δεκεμβρίου" => 12,
            _ => 0
        };
    }

    private static void FlushRecord(
        PharmacyRecordBuilder? builder,
        DutyType? dutyType,
        FsthParseResult result)
    {
        if (builder == null || dutyType == null)
            return;

        var pharmacy =
            builder.Build(
                result.DutyDate,
                dutyType.Value);

        // Ignore pharmacy records without a phone number.
        if (string.IsNullOrWhiteSpace(pharmacy.Phone))
            return;

        result.Pharmacies.Add(pharmacy);
    }

    private static void Validate(
        FsthParseResult result)
    {
        if (result.DutyDate == default)
        {
            result.Warnings.Add(
                "Duty date was not detected.");
        }

        if (result.Pharmacies.Count == 0)
        {
            result.Warnings.Add(
                "No pharmacies were detected.");
            return;
        }

        foreach (var pharmacy in result.Pharmacies)
        {
            if (string.IsNullOrWhiteSpace(pharmacy.Area))
            {
                result.Warnings.Add(
                    $"Missing area for pharmacy '{pharmacy.Name}'.");
            }

            if (string.IsNullOrWhiteSpace(pharmacy.Name))
            {
                result.Warnings.Add(
                    "A pharmacy is missing its name.");
            }

            if (string.IsNullOrWhiteSpace(pharmacy.Address))
            {
                result.Warnings.Add(
                    $"Missing address for pharmacy '{pharmacy.Name}'.");
            }
        }

        var duplicates = result.Pharmacies
            .GroupBy(x => new
            {
                x.DutyType,
                x.Name,
                x.Address,
                x.Phone
            })
            .Where(x => x.Count() > 1)
            .ToList();

        foreach (var duplicate in duplicates)
        {
            result.Warnings.Add(
                $"Duplicate pharmacy detected: " +
                $"{duplicate.Key.Name} - " +
                $"{duplicate.Key.Address} - " +
                $"{duplicate.Key.Phone}");
        }
    }

    private sealed class PdfWord
    {
        public string Text { get; init; } = "";
        public double X { get; init; }
        public double Y { get; init; }
    }

    private sealed class PdfLine
    {
        public double Y { get; init; }

        public List<PdfWord> Words { get; set; } = new();

        public string Text { get; set; } = "";
    }

    private sealed class PharmacyRecordBuilder
    {
        private readonly List<PdfLine> _lines = new();

        public PharmacyRecordBuilder(
            PdfLine firstLine)
        {
            _lines.Add(firstLine);
        }

        public void AddContinuation(
            PdfLine line)
        {
            _lines.Add(line);
        }

        public DutyPharmacy Build(
            DateOnly dutyDate,
            DutyType dutyType)
        {
            var areaParts = new List<string>();
            var nameParts = new List<string>();
            var addressParts = new List<string>();

            foreach (var line in _lines)
            {
                foreach (var word in line.Words)
                {
                    if (word.X < 100)
                    {
                        areaParts.Add(word.Text);
                    }
                    else if (word.X >= 100 &&
                             word.X < 300)
                    {
                        nameParts.Add(word.Text);
                    }
                    else
                    {
                        addressParts.Add(word.Text);
                    }
                }
            }

            var phone =
                ExtractPhone(
                    string.Join(
                        " ",
                        _lines.Select(x => x.Text)));

            var area =
                string.Join(" ", areaParts).Trim();

            var name =
                string.Join(" ", nameParts).Trim();

            var address =
                string.Join(" ", addressParts).Trim();

            // Fallback in case PDF geometry doesn't
            // give us the expected columns.
            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(address))
            {
                var allText =
                    string.Join(
                        " ",
                        _lines.Select(x => x.Text));

                var phoneIndex =
                    phone.Length > 0
                        ? allText.IndexOf(
                            phone,
                            StringComparison.Ordinal)
                        : -1;

                if (phoneIndex >= 0)
                {
                    allText =
                        allText.Remove(
                            phoneIndex,
                            phone.Length);
                }

                var parts =
                    allText
                        .Split(
                            ' ',
                            StringSplitOptions.RemoveEmptyEntries);

                if (string.IsNullOrWhiteSpace(name) &&
                    parts.Length > 0)
                {
                    name = parts[0];
                }

                if (string.IsNullOrWhiteSpace(address) &&
                    parts.Length > 1)
                {
                    address =
                        string.Join(
                            " ",
                            parts.Skip(1));
                }
            }

            return new DutyPharmacy
            {
                DutyDate = dutyDate,
                DutyType = dutyType,
                Area = area,
                Name = name,
                Address = address,
                Phone = phone
            };
        }

        private static string ExtractPhone(
            string text)
        {
            var match =
                PhoneRegex.Match(text);

            return match.Success
                ? match.Value
                : "";
        }
    }
}