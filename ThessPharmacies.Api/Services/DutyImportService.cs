using Microsoft.EntityFrameworkCore;
using ThessPharmacies.Api.Data;
using ThessPharmacies.Api.Data.Entities;
using ThessPharmacies.Parser;

namespace ThessPharmacies.Api.Services;

public sealed class DutyImportService
{
    private readonly ThessPharmaciesDbContext _db;
    private readonly FsthPdfParser _parser;

    private readonly IGeocodingService _geocodingService;

    public DutyImportService(
        ThessPharmaciesDbContext db,
        FsthPdfParser parser,
        IGeocodingService geocodingService)
    {
        _db = db;
        _parser = parser;
        _geocodingService = geocodingService;
    }

    public async Task<FsthParseResult> ImportAsync(
        string pdfPath,
        string sourceFileName,
        CancellationToken cancellationToken = default)
    {
        var result = _parser.Parse(pdfPath);

        if (!result.IsValid)
        {
            return result;
        }

        // Prevent importing the same duty date twice.
        var existingImport = await _db.DutyImports
            .SingleOrDefaultAsync(
                x => x.DutyDate == result.DutyDate,
                cancellationToken);

        if (existingImport != null)
        {
            result.Warnings.Add(
                $"Duty date {result.DutyDate:yyyy-MM-dd} has already been imported.");

            return result;
        }

        await using var transaction =
            await _db.Database.BeginTransactionAsync(
                cancellationToken);

        foreach (var parsedPharmacy in result.Pharmacies)
        {
            var pharmacy = await FindOrCreatePharmacyAsync(
                parsedPharmacy,
                cancellationToken);

            try
            {
                var geocodingResult = await _geocodingService.GeocodeAsync(
                pharmacy.Address,
                cancellationToken);

                if (geocodingResult != null)
                {
                    pharmacy.Latitude = geocodingResult.Latitude;
                    pharmacy.Longitude = geocodingResult.Longitude;
                }
            }
            catch (Exception ex)
            {
                result.Warnings.Add(
                    $"Failed to geocode address '{pharmacy.Address}': {ex.Message}");
            }

            var dutyExists = await _db.PharmacyDuties.AnyAsync(
                x =>
                    x.PharmacyId == pharmacy.Id &&
                    x.DutyDate == parsedPharmacy.DutyDate &&
                    x.DutyType == parsedPharmacy.DutyType,
                cancellationToken);

            if (!dutyExists)
            {
                _db.PharmacyDuties.Add(
                    new PharmacyDuty
                    {
                        Pharmacy = pharmacy,
                        DutyDate = parsedPharmacy.DutyDate,
                        DutyType = parsedPharmacy.DutyType
                    });
            }
        }

        _db.DutyImports.Add(
            new DutyImport
            {
                DutyDate = result.DutyDate,
                ImportedAt = DateTime.UtcNow,
                SourceFileName = sourceFileName,
                PharmacyCount = result.Pharmacies.Count,
                IsValid = result.IsValid
            });

        await _db.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return result;
    }

    private async Task<Pharmacy> FindOrCreatePharmacyAsync(
    DutyPharmacy parsedPharmacy,
    CancellationToken cancellationToken)
    {
        var pharmacy = _db.Pharmacies.Local
            .FirstOrDefault(
                x =>
                    x.Name == parsedPharmacy.Name &&
                    x.Address == parsedPharmacy.Address &&
                    x.Phone == parsedPharmacy.Phone);

        if (pharmacy != null)
        {
            return pharmacy;
        }

        pharmacy = await _db.Pharmacies
            .SingleOrDefaultAsync(
                x =>
                    x.Name == parsedPharmacy.Name &&
                    x.Address == parsedPharmacy.Address &&
                    x.Phone == parsedPharmacy.Phone,
                cancellationToken);

        if (pharmacy != null)
        {
            return pharmacy;
        }

        pharmacy = new Pharmacy
        {
            Name = parsedPharmacy.Name,
            Address = parsedPharmacy.Address,
            Phone = parsedPharmacy.Phone,
            Area = parsedPharmacy.Area
        };

        _db.Pharmacies.Add(pharmacy);

        return pharmacy;
    }
}