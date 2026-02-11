using System.Threading.Tasks;
using CarRental.Domain.Entities;

namespace CarRental.Domain.Interfaces
{
    public interface IBookingRepository
    {
        Task AddAsync(Booking booking);
    }
}
