using Everwell.DAL.Data.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace Everwell.DAL.Data.Requests.TestResult
{
    public class UpdateTestResultRequest
    {
        /// <summary>
        /// Update the outcome of the test (null if not changing)
        /// </summary>
        public ResultOutcome? Outcome { get; set; }
        
        /// <summary>
        /// Update comments on the test result (null if not changing)
        /// </summary>
        [StringLength(500, ErrorMessage = "Comments cannot exceed 500 characters")]
        public string? Comments { get; set; }
        
        /// <summary>
        /// Indicates if follow-up is required (null if not changing)
        /// </summary>
        // public bool? RequiresFollowUp { get; set; }
        
        /// <summary>
        /// ID of the staff member processing the result (null if not changing)
        /// </summary>
        public Guid? StaffId { get; set; }
        
        /// <summary>
        /// When the result was processed (null if not changing)
        /// </summary>
        public DateTime? ProcessedAt { get; set; }
        
        /// <summary>
        /// Has notification been sent to the patient (null if not changing)
        /// </summary>
        // public bool? NotificationSent { get; set; }
        
        /// <summary>
        /// Update the test parameters (null if not changing)
        /// </summary>
        public TestParameter[]? Parameter { get; set; }
    }
}