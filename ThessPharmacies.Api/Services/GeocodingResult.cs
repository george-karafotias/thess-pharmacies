namespace ThessPharmacies.Api.Services
{
    public class GeocodingResult
    {
        public double Latitude { get; init; }
        public double Longitude { get; init; }

        public string? DisplayName { get; init; }

        public string? Street { get; init; }
        public string? HouseNumber { get; init; }
        public string? City { get; init; }
        public string? District { get; init; }
        public string? Postcode { get; init; }

        public GeocodingConfidence Confidence { get; init; }
    }

    public enum GeocodingConfidence
    {
        Unknown,
        Low,
        Medium,
        High
    }
}
