using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Responses.User;

namespace Everwell.DAL.Data.Responses.Appointments;

public class GetAppointmentConsultantResponse
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }
    public GetUserResponse Customer { get; set; }

    public Guid ServiceId { get; set; } // to do: GetServiceResponse

    public DateOnly AppointmentDate { get; set; }

    public ShiftSlot Slot { get; set; }

    public AppointmentStatus Status { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
}