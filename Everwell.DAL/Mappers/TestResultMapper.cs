using AutoMapper;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Requests.TestResult;
using Everwell.DAL.Data.Responses.TestResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everwell.DAL.Mappers
{
    public class TestResultMapper : Profile 
    {
        public TestResultMapper()
        {
            CreateMap<CreateTestResultRequest, TestResult>()
                .ForMember(dest => dest.STITestingId, opt => opt.MapFrom(src => src.STITestingId))
                .ForMember(dest => dest.ResultData, opt => opt.MapFrom(src => src.ResultData))   // You can map if needed
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
                .ForMember(dest => dest.StaffId, opt => opt.MapFrom(src => src.StaffId))
                .ForMember(dest => dest.ExaminedAt, opt => opt.MapFrom(src => src.ExaminedAt))
                .ForMember(dest => dest.SentAt, opt => opt.MapFrom(src => src.SentAt));

            CreateMap<TestResult, CreateTestResultResponse>()
                .ForMember(dest => dest.STITestingId, opt => opt.MapFrom(src => src.STITestingId))
                .ForMember(dest => dest.ResultData, opt => opt.MapFrom(src => src.ResultData))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
                .ForMember(dest => dest.StaffId, opt => opt.MapFrom(src => src.StaffId))
                .ForMember(dest => dest.ExaminedAt, opt => opt.MapFrom(src => src.ExaminedAt))
                .ForMember(dest => dest.SentAt, opt => opt.MapFrom(src => src.SentAt))
                .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => src.Customer))
                .ForMember(dest => dest.Staff, opt => opt.MapFrom(src => src.Staff));

        }
    }
}
