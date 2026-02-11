using CarRental.Domain.Common;

namespace CarRental.Domain.Entities;

public class ServiceLog : BaseEntity
{
    public int CarId { get; set; }
    public DateTime ServiceDate { get; set; }
    public string ServiceType { get; set; }
    public int MileageAtService { get; set; }
    public decimal Cost { get; set; }
}