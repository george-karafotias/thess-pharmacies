using Microsoft.EntityFrameworkCore;
using ThessPharmacies.Api.Data;
using ThessPharmacies.Api.Data.Entities;
using ThessPharmacies.Parser;

namespace ThessPharmacies.Api.Services;

public sealed class PharmacyQueryService
{
    private readonly ThessPharmaciesDbContext _db;

    public PharmacyQueryService(ThessPharmaciesDbContext db)
    {
        _db = db;
    }

    public async Task<List<PharmacyDutyResult>> GetDutiesAsync(
        DateOnly date,
        string? area,
        DutyType? dutyType,
        CancellationToken cancellationToken)
    {
        var query = _db.PharmacyDuties
            .AsNoTracking()
            .Include(x => x.Pharmacy)
            .Where(x => x.DutyDate == date);

        if (!string.IsNullOrWhiteSpace(area))
        {
            query = query.Where(x =>
                x.Pharmacy.Area == area);
        }

        if (dutyType.HasValue)
        {
            query = query.Where(x =>
                x.DutyType == dutyType.Value);
        }

        return await query
            .OrderBy(x => x.Pharmacy.Area)
            .ThenBy(x => x.Pharmacy.Name)
            .Select(x => new PharmacyDutyResult
            {
                PharmacyId = x.PharmacyId,
                DutyDate = x.DutyDate,
                DutyType = x.DutyType,
                Name = x.Pharmacy.Name,
                Address = x.Pharmacy.Address,
                Phone = x.Pharmacy.Phone,
                Area = x.Pharmacy.Area,
                Latitude = x.Pharmacy.Latitude,
                Longitude = x.Pharmacy.Longitude
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<NearbyPharmacyResult>> GetNearbyAsync(
    double latitude,
    double longitude,
    double radiusKm,
    DateOnly date,
    DutyType? dutyType,
    CancellationToken cancellationToken)
    {
        var query = _db.PharmacyDuties
            .AsNoTracking()
            .Include(x => x.Pharmacy)
            .Where(x =>
                x.DutyDate == date &&
                x.Pharmacy.Latitude.HasValue &&
                x.Pharmacy.Longitude.HasValue);

        if (dutyType.HasValue)
        {
            query = query.Where(x =>
                x.DutyType == dutyType.Value);
        }

        var duties = await query
            .Select(x => new NearbyPharmacyResult
            {
                PharmacyId = x.PharmacyId,
                DutyDate = x.DutyDate,
                DutyType = x.DutyType,
                Name = x.Pharmacy.Name,
                Address = x.Pharmacy.Address,
                Phone = x.Pharmacy.Phone,
                Area = x.Pharmacy.Area,
                Latitude = x.Pharmacy.Latitude!.Value,
                Longitude = x.Pharmacy.Longitude!.Value
            })
            .ToListAsync(cancellationToken);

        return duties
            .Select(x =>
            {
                x.DistanceKm = CalculateDistanceKm(
                    latitude,
                    longitude,
                    x.Latitude,
                    x.Longitude);

                return x;
            })
            .Where(x => x.DistanceKm <= radiusKm)
            .OrderBy(x => x.DistanceKm)
            .ToList();
    }

    private static double CalculateDistanceKm(
        double latitude1,
        double longitude1,
        double latitude2,
        double longitude2)
    {
        const double earthRadiusKm = 6371.0;

        var latitude1Radians = DegreesToRadians(latitude1);
        var latitude2Radians = DegreesToRadians(latitude2);

        var deltaLatitude = DegreesToRadians(
            latitude2 - latitude1);

        var deltaLongitude = DegreesToRadians(
            longitude2 - longitude1);

        var a =
            Math.Sin(deltaLatitude / 2) *
            Math.Sin(deltaLatitude / 2) +
            Math.Cos(latitude1Radians) *
            Math.Cos(latitude2Radians) *
            Math.Sin(deltaLongitude / 2) *
            Math.Sin(deltaLongitude / 2);

        var c = 2 * Math.Atan2(
            Math.Sqrt(a),
            Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
}

public sealed class PharmacyDutyResult
{
    public int PharmacyId { get; init; }

    public DateOnly DutyDate { get; init; }

    public DutyType DutyType { get; init; }

    public string Name { get; init; } = "";

    public string Address { get; init; } = "";

    public string Phone { get; init; } = "";

    public string Area { get; init; } = "";

    public double? Latitude { get; init; }

    public double? Longitude { get; init; }
}

public sealed class NearbyPharmacyResult
{
    public int PharmacyId { get; init; }

    public DateOnly DutyDate { get; init; }

    public DutyType DutyType { get; init; }

    public string Name { get; init; } = "";

    public string Address { get; init; } = "";

    public string Phone { get; init; } = "";

    public string Area { get; init; } = "";

    public double Latitude { get; init; }

    public double Longitude { get; init; }

    public double DistanceKm { get; set; }
}