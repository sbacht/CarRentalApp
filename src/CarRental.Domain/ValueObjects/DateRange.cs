using System;
using CarRental.Domain.Common;

namespace CarRental.Domain.ValueObjects
{
    public record DateRange
    {
        public DateTime Start { get; init; }
        public DateTime End { get; init; }

        public DateRange(DateTime start, DateTime end)
        {
            if (end <= start)
                throw new DomainException("End date must be after start date.");

            Start = start;
            End = end;
        }

        public bool OverlapsWith(DateRange other)
        {
            return Start < other.End && End > other.Start;
        }

        public int DurationInDays => (End - Start).Days;
    }
    
    
}
