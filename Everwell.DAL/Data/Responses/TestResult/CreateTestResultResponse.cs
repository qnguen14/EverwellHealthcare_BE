using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Responses.STITests;
using Everwell.DAL.Data.Responses.User;
using System;

namespace Everwell.DAL.Data.Responses.TestResult
{
    public class CreateTestResultResponse
    {
        public Guid Id { get; set; }
        
        public DateOnly? ScheduleDate { get; set; }
        
        public Guid STITestingId { get; set; }
        
        public TestParameter Parameter { get; set; }
        
        public ResultOutcome Outcome { get; set; }
        
        public string? Comments { get; set; }
        
        public Guid? StaffId { get; set; }
        
        public GetUserResponse Staff { get; set; }
        
        public DateTime? ProcessedAt { get; set; }
    }
}
