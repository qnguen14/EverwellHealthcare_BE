using Everwell.DAL.Data.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace Everwell.DAL.Data.Requests.STITests
{
    public class UpdateSTITestRequest
    {
        /// <summary>
        /// The new status of the STI testing (null if not changing)
        /// </summary>
        public TestingStatus Status { get; set; }
        
        /// <summary>
        /// New notes or comments about the STI testing (null if not changing)
        /// </summary>
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
        
        /// <summary>
        /// Update if the STI testing is paid or not (null if not changing)
        /// </summary>
        // public bool? IsPaid { get; set; }
        
        /// <summary>
        /// Update scheduled date if needed (null if not changing)
        /// </summary>
        public DateOnly? ScheduledDate { get; set; }
        
        /// <summary>
        /// Update scheduled time slot if needed (null if not changing)
        /// </summary>
        public ShiftSlot Slot { get; set; }
    }
}