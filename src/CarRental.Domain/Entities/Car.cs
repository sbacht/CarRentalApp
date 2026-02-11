using System.Text.Json; 
using CarRental.Common.Enums;
using CarRental.Domain.Common;
using CarRental.Domain.ValueObjects;

namespace CarRental.Domain.Entities
{
    public class Car : BaseEntity
    {
        // Core Data 
        public string Brand { get; set; }
        public string Model { get; set; }
        public string VinCode { get; set; } 
        public string LicensePlate { get; set; } 
        public int Year { get; set; }
        public string Location { get; set; }
        public decimal BasePrice { get; set; }

        // Search Criteria 
        public Transmission TransmissionType { get; set; }
        public FuelType Fuel { get; set; }
        public CarClass Class { get; set; }
        public int SeatsCount { get; set; }

        // Status & Operations
        public CarStatus Status { get; private set; } = CarStatus.Available;
        public int CurrentMileage { get; set; }

        // Metadata
        public string FeaturesJson { get; set; } = "{}";

        private readonly List<Booking> _bookings = new();
        public IReadOnlyList<Booking> Bookings => _bookings.AsReadOnly();
        
        public Car(string brand, string model, string vinCode, string location, decimal basePrice = 50)
        {
            Brand = brand;
            Model = model;
            VinCode = vinCode;
            Location = location;
            BasePrice = basePrice;
        }
        
        public bool IsAvailable(DateRange requestedRange)
        {
            if (Status == CarStatus.Maintenance || Status == CarStatus.Retired)
                return false;

            return _bookings.All(b => !b.Period.OverlapsWith(requestedRange));
        }
        
        
        public Dictionary<string, string> GetFeatures()
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(FeaturesJson) 
                   ?? new Dictionary<string, string>();
        }
    }
}
