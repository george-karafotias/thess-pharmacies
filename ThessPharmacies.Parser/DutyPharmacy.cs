namespace ThessPharmacies.Parser;

public sealed class DutyPharmacy
{
    public DateOnly DutyDate { get; init; }

    public string Area { get; init; } = "";
    public string Name { get; init; } = "";
    public string Address { get; init; } = "";
    public string Phone { get; init; } = "";

    public DutyType DutyType { get; init; }

    public override string ToString()
    {
        return $"{DutyDate:yyyy-MM-dd} | {DutyType} | " +
               $"{Area} | {Name} | {Address} | {Phone}";
    }
}