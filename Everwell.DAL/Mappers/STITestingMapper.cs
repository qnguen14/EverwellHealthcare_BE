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
                .ForMember(dest => dest.AppointmentId, opt => opt.MapFrom(src => src.AppointmentId))
                .ForMember(dest => dest.TestType, opt => opt.MapFrom(src => src.TestType))
                .ForMember(dest => dest.Method, opt => opt.MapFrom(src => src.Method))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.CollectedDate, opt => opt.MapFrom(src => src.CollectedDate))
                .ForMember(dest => dest.Appointment, opt => opt.Ignore())
                .ForMember(dest => dest.TestResults, opt => opt.Ignore());

            // Map from STITesting to CreateSTITestResponse
            CreateMap<STITesting, CreateSTITestResponse>()
                .ForMember(dest => dest.AppointmentId, opt => opt.MapFrom(src => src.AppointmentId))
                .ForMember(dest => dest.Appointment, opt => opt.MapFrom(src => src.Appointment))
                .ForMember(dest => dest.TestType, opt => opt.MapFrom(src => src.TestType))
                .ForMember(dest => dest.Method, opt => opt.MapFrom(src => src.Method))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.CollectedDate, opt => opt.MapFrom(src => src.CollectedDate));
        }
    }
}