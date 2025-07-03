using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Responses.User;

namespace Everwell.DAL.Data.Responses.Appointments;

public class CheckInResponse
{
    public Guid Id { get; set; }
    public AppointmentStatus Status { get; set; } // e.g., "Checked In", "Checked Out"
    public GetUserResponse Customer { get; set; }
    public GetUserResponse Consultant { get; set; }
    public DateTime? CheckInTime { get; set; } 
}