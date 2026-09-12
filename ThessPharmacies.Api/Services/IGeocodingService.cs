namespace ThessPharmacies.Api.Services
{
    public interface IGeocodingService
    {
        Task<GeocodingResult?> GeocodeAsync(
            string address,
            CancellationToken cancellationToken = default);
    }
}
