using System.Collections.Generic;
using System.Threading.Tasks;
using CarRental.Domain.Entities;

namespace CarRental.Domain.Interfaces
{
    public interface ICarRepository
    {
        Task<Car?> GetByIdAsync(int id);
        Task<List<Car>> GetByLocationAsync(string location);
    }
}
