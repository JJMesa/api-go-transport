using AutoMapper;
using GoTransport.Application.Dtos.Manufacturer;
using GoTransport.Domain.Entities.Bas;

namespace GoTransport.Application.Mappings;

public class ManufacturerMappingProfile : Profile
{
    public ManufacturerMappingProfile()
    {
        CreateMap<Manufacturer, ManufacturerDto>().ReverseMap();
    }
}