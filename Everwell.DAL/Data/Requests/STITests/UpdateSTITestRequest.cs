using Everwell.DAL.Data.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace Everwell.DAL.Data.Requests.STITests
{
    public class UpdateSTITestRequest
    {
        [Required]
        public TestingStatus? Status { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")] 
        public string? Notes { get; set; }
        
        [Required]
        public decimal? TotalPrice { get; set; }
        
        [Required]
        public bool? IsPaid { get; set; }

        [Required]
        public DateOnly? ScheduledDate { get; set; }
        
        [Required]
        public ShiftSlot Slot { get; set; }
    }
}