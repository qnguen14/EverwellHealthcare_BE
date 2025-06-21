using System.ComponentModel.DataAnnotations;
using Everwell.DAL.Data.Entities;

namespace Everwell.DAL.Data.Requests.Appointments;

public class CreateAppointmentRequest
{
    [Required(ErrorMessage = "Customer is required")]
    public Guid CustomerId { get; set; }
    
    [Required(ErrorMessage = "Consultant is required")]
    public Guid ConsultantId { get; set; }
    
    [Required(ErrorMessage = "Appointment date is required")]
    public DateOnly AppointmentDate { get; set; }
    
    [Required(ErrorMessage = "Shift Slot is required")]
    public ShiftSlot Slot { get; set; }
    
    public string Notes { get; set; } = string.Empty;
    
}