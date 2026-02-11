using CarRental.Domain.Common;

namespace CarRental.Domain.Entities;

public enum DamageSeverity
{
    Minor,
    Moderate,
    Severe
}

public class CarDamage : BaseEntity
{
    public int CarId { get; set; }
    public DateTime ReportedDate { get; set; }
    public string Description { get; set; }
    public DamageSeverity Severity { get; set; }
    public bool IsRepaired { get; set; }
}

