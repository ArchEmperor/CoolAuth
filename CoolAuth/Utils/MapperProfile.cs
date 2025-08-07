using System.Net;
using AutoMapper;
using CoolAuth.Data.Entities;
using CoolAuth.DTO;

namespace CoolAuth.Utils;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Session, SessionCacheDto>()
            .ForMember(dest => dest.IpAddress,
                opt => opt.MapFrom(src => src.IpAddress.ToString()))
            .ReverseMap()
            .ForMember(dest => dest.IpAddress, 
                opt => opt.MapFrom(src => IPAddress.Parse(src.IpAddress)));
    }
}