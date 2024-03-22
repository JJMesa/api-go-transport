using AutoMapper;
using GoTransport.Application.Dtos.Route;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Mappings;

public class RouteMappingProfile : Profile
{
    public RouteMappingProfile()
    {
        CreateMap<Route, RouteDto>().ReverseMap();

        CreateMap<Route, RouteCreationDto>().ReverseMap();
        CreateMap<Route, RouteUpdateDto>().ReverseMap();
    }
}