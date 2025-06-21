using AutoMapper;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Requests.TestResult;
using Everwell.DAL.Data.Responses.TestResult;

namespace Everwell.DAL.Mappers
{
    public class TestResultMapper : Profile 
    {
        public TestResultMapper()
        {
            // Map from CreateTestResultRequest to TestResult
            CreateMap<CreateTestResultRequest, TestResult>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.STITestingId, opt => opt.MapFrom(src => src.STITestingId))
                .ForMember(dest => dest.Parameter, opt => opt.MapFrom(src => src.Parameter))
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments))
                .ForMember(dest => dest.StaffId, opt => opt.Ignore())
                .ForMember(dest => dest.ProcessedAt, opt => opt.Ignore())
                .ForMember(dest => dest.STITesting, opt => opt.Ignore())
                .ForMember(dest => dest.Staff, opt => opt.Ignore());

            // Map from TestResult to CreateTestResultResponse
            CreateMap<TestResult, CreateTestResultResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.STITestingId, opt => opt.MapFrom(src => src.STITestingId))
                .ForMember(dest => dest.Parameter, opt => opt.MapFrom(src => src.Parameter))
                .ForMember(dest => dest.Outcome, opt => opt.MapFrom(src => src.Outcome))
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments))
                .ForMember(dest => dest.StaffId, opt => opt.MapFrom(src => src.StaffId))
                .ForMember(dest => dest.Staff, opt => opt.MapFrom(src => src.Staff))
                .ForMember(dest => dest.ProcessedAt, opt => opt.MapFrom(src => src.ProcessedAt))
                .ForMember(dest => dest.ScheduleDate, opt => opt.MapFrom(src => src.STITesting.ScheduleDate));
        }
    }
}
