namespace ThessPharmacies.Api.Data.Entities;

public sealed class Pharmacy
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Address { get; set; } = "";

    public string Phone { get; set; } = "";

    public string Area { get; set; } = "";

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public ICollection<PharmacyDuty> Duties { get; set; } = new List<PharmacyDuty>();
}