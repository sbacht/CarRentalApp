using System;
using CarRental.Domain.Common;
using CarRental.Domain.ValueObjects;

namespace CarRental.Domain.Entities
{
    public class Booking : BaseEntity
    {
        public int CarId { get; private set; }
        public DateRange Period { get; private set; }
        public DateTime CreatedAt { get; private set; }

        // Parameterless constructor for EF Core and serialization
        private Booking() { }

        public Booking(int carId, DateRange period)
        {
            CarId = carId;
            Period = period;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
