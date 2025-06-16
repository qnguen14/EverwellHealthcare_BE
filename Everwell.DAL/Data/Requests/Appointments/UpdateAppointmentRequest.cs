using System.ComponentModel.DataAnnotations;
using Everwell.DAL.Data.Entities;

namespace Everwell.DAL.Data.Requests.Appointments;

public class UpdateAppointmentRequest
{
    [Required(ErrorMessage = "Appointment date is required")]
    public DateOnly AppointmentDate { get; set; }
    
    [Required(ErrorMessage = "Shift Slot is required")]
    public ShiftSlot Slot { get; set; }
    
    [Required(ErrorMessage = "Appointment Status is required")]
    public AppointmentStatus Status { get; set; }
    
    public string Notes { get; set; }
}