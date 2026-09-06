namespace ThessPharmacies.Api.Data.Entities;

public sealed class DutyImport
{
    public int Id { get; set; }

    public string Area { get; set; } = "";

    public DateOnly DutyDate { get; set; }

    public DateTime ImportedAt { get; set; }

    public string SourceFileName { get; set; } = "";

    public int PharmacyCount { get; set; }

    public bool IsValid { get; set; }
}