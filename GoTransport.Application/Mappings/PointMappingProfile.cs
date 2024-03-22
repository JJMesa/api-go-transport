using AutoMapper;
using GoTransport.Application.Dtos.Point;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Mappings;

public class PointMappingProfile : Profile
{
    public PointMappingProfile()
    {
        CreateMap<Point, PointDto>().ReverseMap();

        CreateMap<Point, PointCreationDto>().ReverseMap();
        CreateMap<Point, PointUpdateDto>().ReverseMap();
    }
}