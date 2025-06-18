using Everwell.DAL.Data.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace Everwell.DAL.Data.Requests.TestResult
{
    public class CreateTestResultRequest
    {
        [Required]
        public Guid STITestingId { get; set; }

        [Required]
        public TestParameter Parameter { get; set; }
        
        [Required]
        public ResultOutcome Outcome { get; set; } = ResultOutcome.Pending;
        
        public string? Comments { get; set; }
        
        public Guid? StaffId { get; set; }
        
        public DateTime? ProcessedAt { get; set; }
    }
}
