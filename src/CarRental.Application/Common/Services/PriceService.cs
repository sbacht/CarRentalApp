using System;
using CarRental.Domain.Interfaces;

namespace CarRental.Application.Common.Services
{
    public class PriceService : IPriceService
    {
        public decimal CalculateCarPrice(decimal basePrice, DateTime start, DateTime end)
        {
            var days = (end.Date - start.Date).Days;
            if (days < 1) days = 1;
            return basePrice * days;
        }
    }
}
