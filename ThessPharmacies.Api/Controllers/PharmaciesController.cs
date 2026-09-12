using Microsoft.AspNetCore.Mvc;
using ThessPharmacies.Api.Services;
using ThessPharmacies.Parser;

namespace ThessPharmacies.Api.Controllers;

[ApiController]
[Route("api/pharmacies")]
public sealed class PharmaciesController : ControllerBase
{
    private readonly PharmacyQueryService _queryService;

    public PharmaciesController(
        PharmacyQueryService queryService)
    {
        _queryService = queryService;
    }

    [HttpGet("duties")]
    public async Task<ActionResult<List<PharmacyDutyResponse>>> GetDuties(
        [FromQuery] DateOnly date,
        [FromQuery] string? area,
        [FromQuery] DutyType? dutyType,
        CancellationToken cancellationToken)
    {
        var results = await _queryService.GetDutiesAsync(
            date,
            area,
            dutyType,
            cancellationToken);

        return Ok(ToResponse(results));
    }

    [HttpGet("duties/today")]
    public async Task<ActionResult<List<PharmacyDutyResponse>>> GetTodayDuties(
        [FromQuery] string? area,
        [FromQuery] DutyType? dutyType,
        CancellationToken cancellationToken)
    {
        var today = GetAthensToday();

        var results = await _queryService.GetDutiesAsync(
            today,
            area,
            dutyType,
            cancellationToken);

        return Ok(ToResponse(results));
    }

    [HttpGet("nearby")]
    public async Task<ActionResult<List<NearbyPharmacyResponse>>> GetNearby(
    [FromQuery] double latitude,
    [FromQuery] double longitude,
    [FromQuery] double radius = 5,
    [FromQuery] DutyType? dutyType = null,
    CancellationToken cancellationToken = default)
    {
        if (latitude < -90 || latitude > 90)
        {
            return BadRequest("Latitude must be between -90 and 90.");
        }

        if (longitude < -180 || longitude > 180)
        {
            return BadRequest("Longitude must be between -180 and 180.");
        }

        if (radius <= 0)
        {
            return BadRequest("Radius must be greater than 0.");
        }

        if (radius > 50)
        {
            return BadRequest("Radius cannot exceed 50 km.");
        }

        var today = GetAthensToday();

        var results = await _queryService.GetNearbyAsync(
            latitude,
            longitude,
            radius,
            today,
            dutyType,
            cancellationToken);

        return Ok(
            results.Select(x => new NearbyPharmacyResponse
            {
                PharmacyId = x.PharmacyId,
                DutyDate = x.DutyDate,
                DutyType = x.DutyType.ToString(),
                Name = x.Name,
                Address = x.Address,
                Phone = x.Phone,
                Area = x.Area,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                DistanceKm = Math.Round(x.DistanceKm, 2)
            }).ToList());
    }

    private static DateOnly GetAthensToday()
    {
        var athensTimeZone = TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows()
                ? "GTB Standard Time"
                : "Europe/Athens");

        var athensNow = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow,
            athensTimeZone);

        return DateOnly.FromDateTime(athensNow);
    }

    private static List<PharmacyDutyResponse> ToResponse(
        IEnumerable<PharmacyDutyResult> results)
    {
        return results
            .Select(x => new PharmacyDutyResponse
            {
                PharmacyId = x.PharmacyId,
                DutyDate = x.DutyDate,
                DutyType = x.DutyType.ToString(),
                Name = x.Name,
                Address = x.Address,
                Phone = x.Phone,
                Area = x.Area,
                Latitude = x.Latitude,
                Longitude = x.Longitude
            })
            .ToList();
    }

    public sealed class PharmacyDutyResponse
    {
        public int PharmacyId { get; init; }

        public DateOnly DutyDate { get; init; }

        public string DutyType { get; init; } = "";

        public string Name { get; init; } = "";

        public string Address { get; init; } = "";

        public string Phone { get; init; } = "";

        public string Area { get; init; } = "";

        public double? Latitude { get; init; }

        public double? Longitude { get; init; }
    }

    public sealed class NearbyPharmacyResponse
    {
        public int PharmacyId { get; init; }

        public DateOnly DutyDate { get; init; }

        public string DutyType { get; init; } = "";

        public string Name { get; init; } = "";

        public string Address { get; init; } = "";

        public string Phone { get; init; } = "";

        public string Area { get; init; } = "";

        public double Latitude { get; init; }

        public double Longitude { get; init; }

        public double DistanceKm { get; init; }
    }
}
