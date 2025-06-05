using AutoMapper;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Requests.Auth;
using Everwell.DAL.Data.Responses.Auth;

namespace Everwell.DAL.Mappers;

public class AuthMapper : Profile
{
    public AuthMapper()
    {
        // LoginRequest to User
        CreateMap<LoginRequest, User>()
            .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));
        
        // LoginResponse to User
        CreateMap<User, LoginResponse>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));

        
        
    }
}