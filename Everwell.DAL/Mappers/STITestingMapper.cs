using AutoMapper;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Requests.STITests;
using Everwell.DAL.Data.Responses.STITests;

namespace Everwell.DAL.Mappers
{
    public class STITestingMapper : Profile
    {
        public STITestingMapper()
        {
            // Map from CreateSTITestRequest to STITesting
            CreateMap<CreateSTITestRequest, STITesting>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                // .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
                .ForMember(dest => dest.TestPackage, opt => opt.MapFrom(src => src.TestPackage))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.ScheduleDate, opt => opt.MapFrom(src => src.ScheduleDate))
                .ForMember(dest => dest.Slot, opt => opt.MapFrom(src => src.Slot))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
                .ForMember(dest => dest.SampleTakenAt, opt => opt.Ignore())
                .ForMember(dest => dest.CompletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.TestResults, opt => opt.Ignore());

            // Map from STITesting to CreateSTITestResponse
            CreateMap<STITesting, CreateSTITestResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
                .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => src.Customer))
                .ForMember(dest => dest.TestPackage, opt => opt.MapFrom(src => src.TestPackage))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.ScheduleDate, opt => opt.MapFrom(src => src.ScheduleDate))
                .ForMember(dest => dest.Slot, opt => opt.MapFrom(src => src.Slot))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
                .ForMember(dest => dest.SampleTakenAt, opt => opt.MapFrom(src => src.SampleTakenAt))
                .ForMember(dest => dest.CompletedAt, opt => opt.MapFrom(src => src.CompletedAt))
                .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src => src.IsCompleted))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));
        }
    }
}