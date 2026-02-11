using CarRental.Common.Enums;

namespace CarRental.Common.Dtos;

public class CarDto
{
    public int Id { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal CarPrice { get; set; }
    public Transmission TransmissionType { get; set; }
    public FuelType Fuel { get; set; }
    public CarClass Class { get; set; }
    public int SeatsCount { get; set; }
    public string FeaturesJson { get; set; } = "{}";
}
