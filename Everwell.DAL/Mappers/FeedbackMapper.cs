using AutoMapper;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Requests.Feedback;
using Everwell.DAL.Data.Responses.Feedback;

namespace Everwell.DAL.Mappers;

public class FeedbackMapper : Profile
{
    public FeedbackMapper()
    {
        // Request to Entity mappings
        CreateMap<CreateFeedbackRequest, Feedback>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore()) // Will be set from user context
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Will be set in service
            .ForMember(dest => dest.Customer, opt => opt.Ignore())
            .ForMember(dest => dest.Consultant, opt => opt.Ignore())
            .ForMember(dest => dest.Appointment, opt => opt.Ignore());

        CreateMap<UpdateFeedbackRequest, Feedback>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
            .ForMember(dest => dest.ConsultantId, opt => opt.Ignore())
            .ForMember(dest => dest.AppointmentId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Customer, opt => opt.Ignore())
            .ForMember(dest => dest.Consultant, opt => opt.Ignore())
            .ForMember(dest => dest.Appointment, opt => opt.Ignore());

        // Entity to Response mappings
        CreateMap<Feedback, FeedbackResponse>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.Name))
            .ForMember(dest => dest.CustomerEmail, opt => opt.MapFrom(src => src.Customer.Email))
            .ForMember(dest => dest.CustomerAvatar, opt => opt.MapFrom(src => src.Customer.AvatarUrl))
            .ForMember(dest => dest.ConsultantName, opt => opt.MapFrom(src => src.Consultant.Name))
            .ForMember(dest => dest.ConsultantEmail, opt => opt.MapFrom(src => src.Consultant.Email))
            .ForMember(dest => dest.ConsultantAvatar, opt => opt.MapFrom(src => src.Consultant.AvatarUrl))
            .ForMember(dest => dest.ConsultantSpecialization, opt => opt.MapFrom(src => "General Practice")) // Default since no specialization field
            .ForMember(dest => dest.AppointmentDate, opt => opt.MapFrom(src => src.Appointment.AppointmentDate.ToDateTime(TimeOnly.MinValue)))
            .ForMember(dest => dest.AppointmentStatus, opt => opt.MapFrom(src => src.Appointment.Status.ToString()));

        // Simple response mapping
        CreateMap<Feedback, CreateFeedbackResponse>()
            .ForMember(dest => dest.FeedbackId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => "Feedback created successfully"))
            .ForMember(dest => dest.IsSuccess, opt => opt.MapFrom(src => true));
    }
} 