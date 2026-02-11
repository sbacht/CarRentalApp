using CarRental.Domain.Common;
using System;

namespace CarRental.Domain.Entities
{
    public enum UnavailabilityReason
    {
        Maintenance,
        Accident,
        Other
    }
    
    public class CarStatusReport : BaseEntity
    {
        public int CarId { get; set; }
        public UnavailabilityReason Reason { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EstimatedEndDate { get; set; }
        public string Comments { get; set; }
    }
}
