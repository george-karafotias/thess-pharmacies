using ThessPharmacies.Parser;

namespace ThessPharmacies.Api.Data.Entities;

public sealed class PharmacyDuty
{
    public int Id { get; set; }

    public int PharmacyId { get; set; }

    public Pharmacy Pharmacy { get; set; } = null!;

    public DateOnly DutyDate { get; set; }

    public DutyType DutyType { get; set; }
}