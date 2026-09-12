using System.Text.Json.Serialization;

namespace ThessPharmacies.Api.Services
{
    public class PhotonResponse
    {
        [JsonPropertyName("features")]
        public List<PhotonFeature> Features { get; set; } = [];
    }

    public class PhotonFeature
    {
        [JsonPropertyName("geometry")]
        public PhotonGeometry Geometry { get; set; } = new();

        [JsonPropertyName("properties")]
        public PhotonProperties Properties { get; set; } = new();
    }

    public class PhotonGeometry
    {
        [JsonPropertyName("coordinates")]
        public double[] Coordinates { get; set; } = [];
    }

    public class PhotonProperties
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("street")]
        public string? Street { get; set; }

        [JsonPropertyName("housenumber")]
        public string? HouseNumber { get; set; }

        [JsonPropertyName("postcode")]
        public string? Postcode { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("district")]
        public string? District { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }
    }
}
