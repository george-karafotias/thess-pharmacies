using Microsoft.EntityFrameworkCore;
using ThessPharmacies.Parser;

namespace ThessPharmacies.Api.Data;

public sealed class PharmacyNameResolver : IPharmacyNameResolver
{
    private readonly ThessPharmaciesDbContext _dbContext;

    public PharmacyNameResolver(
        ThessPharmaciesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public string? FindCanonicalName(
        string parsedName,
        string area,
        string phone)
    {
        if (string.IsNullOrWhiteSpace(parsedName))
            return null;

        var normalizedName = Normalize(parsedName);

        if (string.IsNullOrWhiteSpace(normalizedName))
            return null;

        var pharmacies = _dbContext.Pharmacies
            .AsNoTracking()
            .Where(x =>
                string.IsNullOrWhiteSpace(area) ||
                x.Area == area)
            .ToList();

        foreach (var pharmacy in pharmacies)
        {
            var existingName = Normalize(pharmacy.Name);

            if (existingName.Contains(normalizedName) ||
                normalizedName.Contains(existingName))
            {
                return pharmacy.Name;
            }
        }

        return null;
    }

    private static string Normalize(string value)
    {
        return string.Join(
            " ",
            value
                .ToUpperInvariant()
                .Replace(".", "")
                .Replace("-", " ")
                .Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries));
    }
}