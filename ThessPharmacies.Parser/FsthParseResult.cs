namespace ThessPharmacies.Parser;

public sealed class FsthParseResult
{
    public DateOnly DutyDate { get; set; }

    public string AreaGroup { get; set; } = "";

    public List<DutyPharmacy> Pharmacies { get; } = new();

    public List<string> Warnings { get; } = new();

    public bool IsValid =>
        Pharmacies.Count > 0 &&
        Warnings.Count == 0;

    public int DaytimeCount =>
        Pharmacies.Count(x => x.DutyType == DutyType.Daytime);

    public int OvernightCount =>
        Pharmacies.Count(x => x.DutyType == DutyType.Overnight);

    public int AfterMidnightCount =>
        Pharmacies.Count(x => x.DutyType == DutyType.AfterMidnight);
}