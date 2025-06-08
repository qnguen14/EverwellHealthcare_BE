using Everwell.DAL.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everwell.DAL.Data.Requests.Appointments
{
    public class CreateScheduleRequest
    {   public Guid ConsultantId { get; set; } // Consultant's ID
        public DateOnly WorkDate { get; set; } // Date for the schedule
        public ShiftSlot Slot { get; set; }
        public bool IsAvailable { get; set; } = true; // Availability status    
    }
}
