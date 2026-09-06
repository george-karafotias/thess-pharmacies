using System.Text;
using ThessPharmacies.Parser;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

Console.WriteLine("===============================================");
Console.WriteLine("        ThessPharmacies PDF Parser");
Console.WriteLine("===============================================");
Console.WriteLine();

if (args.Length == 0)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("ERROR: PDF path argument is required.");
    Console.ResetColor();

    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  ThessPharmacies.Console <pdf-path>");
    Console.WriteLine();
    Console.WriteLine("Example:");
    Console.WriteLine(
        @"  ThessPharmacies.Console ""C:\Users\geoka\Downloads\Εφημερίες_06-09-2026.pdf""");

    return;
}

var pdfPath = args[0];

Console.WriteLine($"PDF: {pdfPath}");
Console.WriteLine();

if (!File.Exists(pdfPath))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("ERROR: PDF file was not found.");
    Console.ResetColor();

    return;
}

try
{
    var parser = new FsthPdfParser();

    Console.WriteLine("Parsing PDF...");
    Console.WriteLine();

    var result = parser.Parse(pdfPath);

    Console.WriteLine("===============================================");
    Console.WriteLine("                 PARSE RESULT");
    Console.WriteLine("===============================================");
    Console.WriteLine();

    Console.WriteLine(
        $"Duty date       : {result.DutyDate:dd/MM/yyyy}");

    Console.WriteLine(
        $"Daytime         : {result.DaytimeCount}");

    Console.WriteLine(
        $"Overnight       : {result.OvernightCount}");

    Console.WriteLine(
        $"After midnight  : {result.AfterMidnightCount}");

    Console.WriteLine(
        $"Total           : {result.Pharmacies.Count}");

    Console.WriteLine();

    if (result.IsValid)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("VALIDATION: PASSED");
        Console.ResetColor();
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("VALIDATION: FAILED");
        Console.ResetColor();
    }

    Console.WriteLine();

    if (result.Warnings.Count > 0)
    {
        Console.WriteLine("===============================================");
        Console.WriteLine("                  WARNINGS");
        Console.WriteLine("===============================================");
        Console.WriteLine();

        foreach (var warning in result.Warnings)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"- {warning}");
            Console.ResetColor();
        }

        Console.WriteLine();
    }

    Console.WriteLine("===============================================");
    Console.WriteLine("                 PHARMACIES");
    Console.WriteLine("===============================================");
    Console.WriteLine();

    foreach (var pharmacy in result.Pharmacies)
    {
        Console.WriteLine(
            $"{pharmacy.DutyType,-15} | " +
            $"{pharmacy.Area,-20} | " +
            $"{pharmacy.Name,-35} | " +
            $"{pharmacy.Address,-40} | " +
            $"{pharmacy.Phone}");
    }

    Console.WriteLine();
    Console.WriteLine("===============================================");
    Console.WriteLine("Parsing completed.");
    Console.WriteLine("===============================================");
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;

    Console.WriteLine("===============================================");
    Console.WriteLine("                    ERROR");
    Console.WriteLine("===============================================");
    Console.WriteLine();

    Console.WriteLine(ex.Message);

    Console.ResetColor();
}

Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();