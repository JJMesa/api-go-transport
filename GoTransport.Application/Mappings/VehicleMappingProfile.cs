using AutoMapper;
using GoTransport.Application.Dtos.Vehicle;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Mappings;

public class VehicleMappingProfile : Profile
{
    public VehicleMappingProfile()
    {
        CreateMap<Vehicle, VehicleDto>().ReverseMap();

        CreateMap<Vehicle, VehicleCreationDto>().ReverseMap();
        CreateMap<Vehicle, VehicleUpdateDto>().ReverseMap();
    }
}