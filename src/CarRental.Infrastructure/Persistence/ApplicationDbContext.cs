using CarRental.Application;
using Microsoft.EntityFrameworkCore;
using CarRental.Domain.Entities;
using CarRental.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage;

namespace CarRental.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Car> Cars { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<CarDamage> CarDamages { get; set; }
        public DbSet<ServiceLog> ServiceLogs { get; set; }
        public DbSet<CarStatusReport> CarStatusReports { get; set; }
        
        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
        {
            return await Database.BeginTransactionAsync(cancellationToken);
        }

        public Task CommitTransactionAsync(CancellationToken cancellationToken)
        {
            return Database.CommitTransactionAsync(cancellationToken);
        }

        public Task RollbackTransactionAsync(CancellationToken cancellationToken)
        {
            return Database.RollbackTransactionAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            // Ensure the table names are exactly what you expect
            modelBuilder.Entity<Car>().ToTable("Cars");
            modelBuilder.Entity<Booking>().ToTable("Bookings");
            modelBuilder.Entity<CarDamage>().ToTable("CarDamages");
            modelBuilder.Entity<ServiceLog>().ToTable("ServiceLogs");
            modelBuilder.Entity<CarStatusReport>().ToTable("CarStatusReports");

            // Configure DateRange Value Object in Booking
            modelBuilder.Entity<Booking>().ComplexProperty(b => b.Period);

            // Configure BasePrice for Car
            modelBuilder.Entity<Car>().Property(c => c.BasePrice).HasColumnType("decimal(18,2)");

            // Configure PK/SK for Car related tables
            modelBuilder.Entity<CarDamage>().HasKey(cd => new { cd.CarId, cd.Id });
            modelBuilder.Entity<ServiceLog>().HasKey(sl => new { sl.CarId, sl.Id });
            modelBuilder.Entity<CarStatusReport>().HasKey(cs => new { cs.CarId, cs.Id });
        }
    }
}
