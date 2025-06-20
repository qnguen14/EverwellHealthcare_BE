using Everwell.DAL.Data.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace Everwell.DAL.Data.Requests.TestResult
{
    public class UpdateTestResultRequest
    {
        public ResultOutcome? Outcome { get; set; }
        [StringLength(500, ErrorMessage = "Comments cannot exceed 500 characters")]
        public string? Comments { get; set; }
        public Guid? StaffId { get; set; }
        
        public TestParameter? Parameter { get; set; }
    }
}