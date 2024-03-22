using AutoMapper;
using GoTransport.Application.Dtos.Department;
using GoTransport.Domain.Entities.Bas;

namespace GoTransport.Application.Mappings;

public class DepartmentMappingProfile : Profile
{
    public DepartmentMappingProfile()
    {
        CreateMap<Department, DepartmentDto>().ReverseMap();

        CreateMap<Department, DepartmentCreationDto>().ReverseMap();
        CreateMap<Department, DepartmentUpdateDto>().ReverseMap();
    }
}