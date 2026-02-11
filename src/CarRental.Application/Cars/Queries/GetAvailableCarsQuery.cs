using AutoMapper;
using CarRental.Application.Common.Models;
using CarRental.Common.Dtos;
using CarRental.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Application.Cars.Queries;

public record GetAvailableCarsQuery(string City, DateTime StartDate, DateTime EndDate)
    : IRequest<Result<List<CarDto>>>;

public class GetAvailableCarsQueryValidator : AbstractValidator<GetAvailableCarsQuery>
{
    public GetAvailableCarsQueryValidator()
    {
        RuleFor(x => x.City)
            .NotEmpty();

        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date);

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate);
    }
}

public class GetAvailableCarsQueryHandler : IRequestHandler<GetAvailableCarsQuery, Result<List<CarDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPriceService _priceService;

    public GetAvailableCarsQueryHandler(IApplicationDbContext context, IMapper mapper, IPriceService priceService)
    {
        _context = context;
        _mapper = mapper;
        _priceService = priceService;
    }

    public async Task<Result<List<CarDto>>> Handle(GetAvailableCarsQuery request, CancellationToken cancellationToken)
    {
        var availableCars = await _context.Cars
            .Where(c => c.Location == request.City)
            .Where(c => !c.Bookings.Any(b => 
                b.Period.Start < request.EndDate && 
                b.Period.End > request.StartDate))
            .ToListAsync(cancellationToken);


        var result = availableCars.Select(c => {
            var dto = _mapper.Map<CarDto>(c);
            dto.CarPrice = _priceService.CalculateCarPrice(c.BasePrice, request.StartDate, request.EndDate);
            return dto;
        }).ToList();

        return Result<List<CarDto>>.Success(result);
    }
}
