﻿using AutoMapper;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Requests.User;
using Everwell.DAL.Data.Responses.User;

namespace Everwell.DAL.Mappers
{
    public class UserMapper : Profile
    {
        public UserMapper()
        {
            // CreateUserRequest to User
            CreateMap<CreateUserRequest, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // Id will be generated by the database
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true)) // Default to active
                .ForMember(dest => dest.Password, opt => opt.Ignore()) // Password will be hashed manually
                .ForMember(dest => dest.Role, opt => opt.Ignore()) // Role will be parsed manually
                .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => (string)null)) // Default null
                .ForMember(dest => dest.Posts, opt => opt.Ignore()) // Ignore navigation properties
                .ForMember(dest => dest.STITests, opt => opt.Ignore())
                .ForMember(dest => dest.TestResultsExamined, opt => opt.Ignore());
                // .ForMember(dest => dest.TestResultsSent, opt => opt.Ignore());

            // UpdateUserRequest to User
            CreateMap<UpdateUserRequest, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // Don't update the ID
                .ForMember(dest => dest.IsActive, opt => opt.Ignore()) // Don't update IsActive in normal updates
                .ForMember(dest => dest.Password, opt => opt.Ignore()) // Don't update password in normal updates
                .ForMember(dest => dest.AvatarUrl, opt => opt.Ignore()) // Don't update avatar in normal updates
                .ForMember(dest => dest.Posts, opt => opt.Ignore()) // Don't update related entities
                .ForMember(dest => dest.STITests, opt => opt.Ignore()) // Don't update related entities
                .ForMember(dest => dest.TestResultsExamined, opt => opt.Ignore()); // Don't update related entities
                // .ForMember(dest => dest.TestResultsSent, opt => opt.Ignore()); // Don't update related entities

            // User to CreateUserResponse
            CreateMap<User, CreateUserResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Password, opt => opt.Ignore()) // Don't return password in response
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => 
                    src.Role.Name))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

            // User to UpdateUserResponse
            CreateMap<User, UpdateUserResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.Name))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

            // User to GetUserResponse
            CreateMap<User, GetUserResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.Name))
                .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.AvatarUrl))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

            // CreateUserResponse to GetUserResponse
            CreateMap<CreateUserResponse, GetUserResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => (string)null)) // Default null since CreateUserResponse doesn't have this
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

            // UpdateProfileRequest to User (partial mapping)
            CreateMap<UpdateProfileRequest, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.AvatarUrl, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Posts, opt => opt.Ignore())
                .ForMember(dest => dest.STITests, opt => opt.Ignore())
                .ForMember(dest => dest.TestResultsExamined, opt => opt.Ignore());
                // .ForMember(dest => dest.TestResultsSent, opt => opt.Ignore());

            // User to UserProfileResponse
            CreateMap<User, UserProfileResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.AvatarUrl))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow)) // Default value, can be updated if you add CreatedAt to User entity
                .ForMember(dest => dest.LastLoginAt, opt => opt.MapFrom(src => (DateTime?)null)) // Default null, can be updated if you track last login
                .ForMember(dest => dest.TotalPosts, opt => opt.Ignore()) // Will be set manually in service
                .ForMember(dest => dest.TotalAppointments, opt => opt.Ignore()) // Will be set manually in service
                .ForMember(dest => dest.TotalSTITests, opt => opt.Ignore()); // Will be set manually in service
        }
    }
}
