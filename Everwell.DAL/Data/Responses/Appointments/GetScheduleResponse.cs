using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Responses.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everwell.DAL.Data.Responses.Appointments
{
    public class GetScheduleResponse
    {
        public Guid Id { get; set; } // Unique identifier for the schedule
        public Guid ConsultantId { get; set; }
        public GetUserResponse Consultant { get; set; }// Consultant's ID
        public DateOnly WorkDate { get; set; } // Date for the schedule
        public ShiftSlot Slot { get; set; }
        public bool IsAvailable { get; set; } = true; // Availability status    
        public DateTime CreatedAt { get; set; } 
    }
}
