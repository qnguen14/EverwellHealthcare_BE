using System;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Responses.User;

namespace Everwell.DAL.Data.Responses.STITests
{
    public class CreateSTITestResponse
    {
        public Guid Id { get; set; }
        
        public Guid CustomerId { get; set; }
        
        public GetUserResponse Customer { get; set; }
        
        public TestPackage TestPackage { get; set; }
        
        public TestingStatus Status { get; set; }
        
        public DateOnly ScheduleDate { get; set; }
        
        public ShiftSlot Slot { get; set; }
        
        public string? Notes { get; set; }
        
        public DateTime? SampleTakenAt { get; set; }
        
        public DateTime? CompletedAt { get; set; }
        
        public bool IsCompleted { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
}
