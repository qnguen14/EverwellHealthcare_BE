using System;
using System.ComponentModel.DataAnnotations;
using Everwell.DAL.Data.Entities;

namespace Everwell.DAL.Data.Requests.STITests
{
    public class CreateSTITestRequest
    {
        // [Required]
        // public Guid CustomerId { get; set; }

        [Required]
        public TestPackage TestPackage { get; set; }
        
        public List<TestParameter>? CustomParameters { get; set; }

        [Required]
        public DateOnly ScheduleDate { get; set; }
        
        [Required]
        public ShiftSlot Slot { get; set; }
        
        [Required]
        public decimal TotalPrice { get; set; }
        
        public string? Notes { get; set; }
    }
}