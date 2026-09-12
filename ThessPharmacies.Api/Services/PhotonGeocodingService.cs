namespace ThessPharmacies.Api.Services
{
    public class PhotonGeocodingService : IGeocodingService
    {
        private readonly HttpClient _httpClient;

        public PhotonGeocodingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<GeocodingResult?> GeocodeAsync(
            string address,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(address))
                return null;

            var url = $"https://photon.komoot.io/api/?q={Uri.EscapeDataString(address)}&limit=1";

            using var response = await _httpClient.GetAsync(
                url,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var photon =
                await response.Content.ReadFromJsonAsync<PhotonResponse>(
                    cancellationToken);

            var feature = photon?.Features.FirstOrDefault();

            if (feature == null ||
                feature.Geometry.Coordinates.Length < 2)
            {
                return null;
            }

            // Photon returns [longitude, latitude]
            var longitude = feature.Geometry.Coordinates[0];
            var latitude = feature.Geometry.Coordinates[1];

            return new GeocodingResult
            {
                Latitude = latitude,
                Longitude = longitude,
                DisplayName = feature.Properties.Name,
                Street = feature.Properties.Street,
                HouseNumber = feature.Properties.HouseNumber,
                City = feature.Properties.City,
                District = feature.Properties.District,
                Postcode = feature.Properties.Postcode,
                Confidence = DetermineConfidence(feature.Properties)
            };
        }

        private static GeocodingConfidence DetermineConfidence(
            PhotonProperties properties)
        {
            if (!string.IsNullOrWhiteSpace(properties.Street) &&
                !string.IsNullOrWhiteSpace(properties.HouseNumber) &&
                !string.IsNullOrWhiteSpace(properties.City))
            {
                return GeocodingConfidence.High;
            }

            if (!string.IsNullOrWhiteSpace(properties.Street) &&
                !string.IsNullOrWhiteSpace(properties.City))
            {
                return GeocodingConfidence.Medium;
            }

            if (!string.IsNullOrWhiteSpace(properties.City))
            {
                return GeocodingConfidence.Low;
            }

            return GeocodingConfidence.Unknown;
        }
    }
}
