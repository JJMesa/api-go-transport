using AutoMapper;

using GoTransport.Application.Dtos.User;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Mappings;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>().ReverseMap()
            .ForMember(d => d.Id, o => o.MapFrom(x => x.UserId));
        CreateMap<UserDto, User>().ReverseMap()
            .ForMember(d => d.UserId, o => o.MapFrom(x => x.Id));

        CreateMap<User, UserCreationDto>()
            .ForMember(d => d.Email, o => o.MapFrom(x => x.UserName)).ReverseMap();
        CreateMap<User, UserUpdateDto>()
            .ForMember(d => d.UserId, o => o.MapFrom(x => x.Id)).ReverseMap();
    }
}