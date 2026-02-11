using AutoMapper;
using CarRental.Application.Bookings.Dtos;
using CarRental.Application.Cars.Queries;
using CarRental.Common.Dtos;
using CarRental.Domain.Entities;

namespace CarRental.Application.Common.Mappings;

public class CarRentalProfile : Profile
{
    public CarRentalProfile()
    {
        CreateMap<Car, CarDto>();

        CreateMap<Booking, BookingDto>()
            .ForMember(d => d.StartDate, cfg => cfg.MapFrom(s => s.Period.Start))
            .ForMember(d => d.EndDate, cfg => cfg.MapFrom(s => s.Period.End));
    }
}
