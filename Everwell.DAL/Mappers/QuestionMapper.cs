using AutoMapper;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Requests.Questions;
using Everwell.DAL.Data.Responses.Questions;

namespace Everwell.DAL.Mappers;

public class QuestionMapper : Profile
{
    public QuestionMapper()
    {
        CreateMap<CreateQuestionRequest, Question>()
            .ForMember(dest => dest.QuestionId, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore()) // Will be set from current user
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => QuestionStatus.Pending))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.AnsweredAt, opt => opt.Ignore())
            .ForMember(dest => dest.AnswerText, opt => opt.Ignore())
            .ForMember(dest => dest.Customer, opt => opt.Ignore())
            .ForMember(dest => dest.Consultant, opt => opt.Ignore());

        CreateMap<UpdateQuestionRequest, Question>()
            .ForMember(dest => dest.QuestionId, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
            .ForMember(dest => dest.ConsultantId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Customer, opt => opt.Ignore())
            .ForMember(dest => dest.Consultant, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Question, QuestionResponse>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.Name))
            .ForMember(dest => dest.CustomerEmail, opt => opt.MapFrom(src => src.Customer.Email))
            .ForMember(dest => dest.ConsultantName, opt => opt.MapFrom(src => src.Consultant != null ? src.Consultant.Name : null))
            .ForMember(dest => dest.ConsultantEmail, opt => opt.MapFrom(src => src.Consultant != null ? src.Consultant.Email : null));

        CreateMap<Question, CreateQuestionResponse>()
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => "Question created successfully"))
            .ForMember(dest => dest.IsSuccess, opt => opt.MapFrom(src => true));
    }
} 