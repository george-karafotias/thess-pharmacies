namespace ThessPharmacies.Parser;

public interface IPharmacyNameResolver
{
    string? FindCanonicalName(
        string parsedName,
        string area,
        string phone);
}