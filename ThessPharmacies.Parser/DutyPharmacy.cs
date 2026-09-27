namespace ThessPharmacies.Parser;

public sealed class DutyPharmacy
{
    public DateOnly DutyDate { get; set; }

    public string Area { get; set; } = "";
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public string Phone { get; set; } = "";

    public DutyType DutyType { get; set; }

    public override string ToString()
    {
        return $"{DutyDate:yyyy-MM-dd} | {DutyType} | " +
               $"{Area} | {Name} | {Address} | {Phone}";
    }
}