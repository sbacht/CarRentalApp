using CarRental.Application.Common.Models;
using CarRental.Domain.Entities;
using CarRental.Domain.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Application.Bookings.Commands;

public record CreateBookingCommand(int CarId, DateTime StartDate, DateTime EndDate)
    : IRequest<Result<int>>;

public class CreateBookingValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingValidator()
    {
        RuleFor(x => x.CarId)
            .NotEmpty();

        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date);

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate);
    }
}

public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;

    public CreateBookingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var requestedRange = new DateRange(request.StartDate, request.EndDate);
        
        using var transaction = await _context.BeginTransactionAsync(cancellationToken);

        try
        {
            bool hasOverlap = await _context.Bookings
                .AnyAsync(b => b.CarId == request.CarId &&
                               b.Period.Start < requestedRange.End &&
                               b.Period.End > requestedRange.Start, 
                    cancellationToken);

            if (hasOverlap)
            {
                return Result<int>.Failure("Car is already booked for these dates.");
            }

            var booking = new Booking(request.CarId, requestedRange);
            _context.Bookings.Add(booking);
        
            await _context.SaveChangesAsync(cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);

            return Result<int>.Success(booking.Id);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<int>.Failure("An error occurred during booking.");
        }
    }
}
