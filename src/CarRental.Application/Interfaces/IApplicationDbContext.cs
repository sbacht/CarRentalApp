using CarRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CarRental.Application;

public interface IApplicationDbContext
{
    DbSet<Car> Cars { get; }
    DbSet<Booking> Bookings { get; }

    // This allows us to call SaveChangesAsync in our Handlers
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
    Task CommitTransactionAsync(CancellationToken cancellationToken);
    Task RollbackTransactionAsync(CancellationToken cancellationToken);

}