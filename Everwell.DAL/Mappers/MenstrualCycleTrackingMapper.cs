using AutoMapper;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Requests.MenstrualCycle;
using Everwell.DAL.Data.Responses.MenstrualCycle;

namespace Everwell.DAL.Mappers
{
    public class MenstrualCycleTrackingMapper : Profile
    {
        public MenstrualCycleTrackingMapper()
        {
            // MenstrualCycleTracking to GetMenstrualCycleResponse
            CreateMap<MenstrualCycleTracking, GetMenstrualCycleResponse>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : string.Empty))
                .ForMember(dest => dest.Notifications, opt => opt.MapFrom(src => src.Notifications));

            // MenstrualCycleTracking to CreateMenstrualCycleResponse
            CreateMap<MenstrualCycleTracking, CreateMenstrualCycleResponse>();

            // CreateMenstrualCycleRequest to MenstrualCycleTracking
            CreateMap<CreateMenstrualCycleRequest, MenstrualCycleTracking>()
                .ForMember(dest => dest.TrackingId, opt => opt.Ignore()) // Will be set in service
                .ForMember(dest => dest.CustomerId, opt => opt.Ignore()) // Will be set in service
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Will be set in service
                .ForMember(dest => dest.Customer, opt => opt.Ignore()) // Navigation property
                .ForMember(dest => dest.Notifications, opt => opt.Ignore()); // Navigation property

            // UpdateMenstrualCycleRequest to MenstrualCycleTracking
            CreateMap<UpdateMenstrualCycleRequest, MenstrualCycleTracking>()
                .ForMember(dest => dest.TrackingId, opt => opt.Ignore()) // Don't update ID
                .ForMember(dest => dest.CustomerId, opt => opt.Ignore()) // Don't update customer ID
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Don't update creation date
                .ForMember(dest => dest.Customer, opt => opt.Ignore()) // Navigation property
                .ForMember(dest => dest.Notifications, opt => opt.Ignore()); // Navigation property

            // MenstrualCycleNotification to NotificationResponse
            CreateMap<MenstrualCycleNotification, NotificationResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.NotificationId))
                .ForMember(dest => dest.NotificationType, opt => opt.MapFrom(src => src.Phase.ToString()))
                .ForMember(dest => dest.NotificationDate, opt => opt.MapFrom(src => src.SentAt))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                .ForMember(dest => dest.IsSent, opt => opt.MapFrom(src => true)) // If it exists in DB, it's been sent
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.SentAt)); // Use SentAt as CreatedAt
        }
    }
}