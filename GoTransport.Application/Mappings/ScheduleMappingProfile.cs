using AutoMapper;
using GoTransport.Application.Dtos.Schedule;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Mappings;

public class ScheduleMappingProfile : Profile
{
    public ScheduleMappingProfile()
    {
        CreateMap<Schedule, ScheduleDto>().ReverseMap();

        CreateMap<Schedule, ScheduleCreationDto>().ReverseMap();
        CreateMap<Schedule, ScheduleUpdateDto>().ReverseMap();
    }
}