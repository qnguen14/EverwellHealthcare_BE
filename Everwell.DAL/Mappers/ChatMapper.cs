using AutoMapper;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Requests.Chat;
using Everwell.DAL.Data.Responses.Chat;

namespace Everwell.DAL.Mappers
{
    public class ChatMapper : Profile
    {
        public ChatMapper()
        {
            // Entity to Response
            CreateMap<ChatMessage, ChatMessageResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.AppointmentId, opt => opt.MapFrom(src => src.AppointmentId))
                .ForMember(dest => dest.SenderId, opt => opt.MapFrom(src => src.SenderId))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                .ForMember(dest => dest.SentAt, opt => opt.MapFrom(src => src.SentAt))
                .ForMember(dest => dest.SenderName, opt => opt.MapFrom(src => src.SenderName))
                .ForMember(dest => dest.SenderRole, opt => opt.MapFrom(src => src.SenderRole))
                .ForMember(dest => dest.IsSystemMessage, opt => opt.MapFrom(src => src.IsSystemMessage));

            // Request to Entity
            CreateMap<SendChatMessageRequest, ChatMessage>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.AppointmentId, opt => opt.MapFrom(src => src.AppointmentId))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                .ForMember(dest => dest.IsSystemMessage, opt => opt.MapFrom(src => src.IsSystemMessage))
                .ForMember(dest => dest.SenderId, opt => opt.Ignore())
                .ForMember(dest => dest.SentAt, opt => opt.Ignore())
                .ForMember(dest => dest.SenderName, opt => opt.Ignore())
                .ForMember(dest => dest.SenderRole, opt => opt.Ignore())
                .ForMember(dest => dest.Appointment, opt => opt.Ignore())
                .ForMember(dest => dest.Sender, opt => opt.Ignore());
        }
    }
} 