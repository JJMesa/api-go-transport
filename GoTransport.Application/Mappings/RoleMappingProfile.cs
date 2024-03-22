using AutoMapper;
using GoTransport.Application.Dtos.Role;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Mappings;

public class RoleMappingProfile : Profile
{
    public RoleMappingProfile()
    {
        CreateMap<Role, RoleDto>()
            .ForMember(d => d.RoleId, o => o.MapFrom(x => x.Id)).ReverseMap();
    }
}