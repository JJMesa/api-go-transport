using AutoMapper;
using GoTransport.Application.Dtos.City;
using GoTransport.Domain.Entities.Bas;

namespace GoTransport.Application.Mappings;

public class CityMappingProfile : Profile
{
    public CityMappingProfile()
    {
        CreateMap<City, CityDto>().ReverseMap();

        CreateMap<City, CityCreationDto>().ReverseMap();
        CreateMap<City, CityUpdateDto>().ReverseMap();
    }
}