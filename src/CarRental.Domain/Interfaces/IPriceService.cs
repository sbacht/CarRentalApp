namespace CarRental.Domain.Interfaces
{
    public interface IPriceService
    {
        decimal CalculateCarPrice(decimal basePrice, DateTime start, DateTime end);
    }
}
