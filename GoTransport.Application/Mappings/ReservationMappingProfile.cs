using AutoMapper;
using GoTransport.Application.Dtos.Reservation;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Mappings;

public class ReservationMappingProfile : Profile
{
    public ReservationMappingProfile()
    {
        CreateMap<Reservation, ReservationDto>().ReverseMap();

        CreateMap<Reservation, ReservationCreationDto>().ReverseMap();
        CreateMap<Reservation, ReservationUpdateDto>().ReverseMap();
    }
}