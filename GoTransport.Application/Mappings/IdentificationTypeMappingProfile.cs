using AutoMapper;
using GoTransport.Application.Dtos.IdentificationType;
using GoTransport.Domain.Entities.Bas;

namespace GoTransport.Application.Mappings;

public class IdentificationTypeMappingProfile : Profile
{
    public IdentificationTypeMappingProfile()
    {
        CreateMap<IdentificationType, IdentificationTypeDto>().ReverseMap();
    }
}