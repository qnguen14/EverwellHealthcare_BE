using AutoMapper;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Requests.Notifications;
using Everwell.DAL.Data.Responses.Notifications;

namespace Everwell.DAL.Mappers;

public class NotificationMapper : Profile
{
    public NotificationMapper()
    {
        // Map from Notification entity to GetNotificationResponse
        CreateMap<Notification, GetNotificationResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.IsRead, opt => opt.MapFrom(src => src.IsRead))
            .ForMember(dest => dest.AppointmentId, opt => opt.MapFrom(src => src.AppointmentId))
            .ForMember(dest => dest.TestResultId, opt => opt.MapFrom(src => src.TestResultId))
            .ForMember(dest => dest.STITestingId, opt => opt.MapFrom(src => src.STITestingId));

        // Map from CreateNotificationRequest to Notification entity
        CreateMap<CreateNotificationRequest, Notification>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority))
            .ForMember(dest => dest.AppointmentId, opt => opt.MapFrom(src => src.AppointmentId))
            .ForMember(dest => dest.TestResultId, opt => opt.MapFrom(src => src.TestResultId))
            .ForMember(dest => dest.STITestingId, opt => opt.MapFrom(src => src.STITestingId))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.IsRead, opt => opt.MapFrom(_ => false))
            .ForMember(dest => dest.Customer, opt => opt.Ignore())
            .ForMember(dest => dest.Appointment, opt => opt.Ignore())
            .ForMember(dest => dest.TestResult, opt => opt.Ignore());
    }
}