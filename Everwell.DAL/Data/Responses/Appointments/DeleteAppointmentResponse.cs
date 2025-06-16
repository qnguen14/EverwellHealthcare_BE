namespace Everwell.DAL.Data.Responses.Appointments;

public class DeleteAppointmentResponse
{
    public Guid AppointmentId { get; set; }
    public bool IsDeleted { get; set; }
    public string Message { get; set; }

    public DeleteAppointmentResponse(Guid appointmentId, bool isDeleted, string message)
    {
        AppointmentId = appointmentId;
        IsDeleted = isDeleted;
        Message = message;
    }
    
    public DeleteAppointmentResponse() { }
}