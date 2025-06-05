 using Everwell.DAL.Data.Entities;
 using Everwell.DAL.Data.Requests.Appointments;
 using Everwell.DAL.Data.Responses.Appointments;

 namespace Everwell.BLL.Services.Interfaces;

public interface IAppointmentService
{
    Task<IEnumerable<CreateAppointmentsResponse>> GetAllAppointmentsAsync();
    Task<CreateAppointmentsResponse> GetAppointmentByIdAsync(Guid id); // to do: getappointmentresponse
    Task<CreateAppointmentsResponse> CreateAppointmentAsync(CreateAppointmentRequest request);
    Task<Appointment?> UpdateAppointmentAsync(Guid id, Appointment appointment);
    Task<bool> DeleteAppointmentAsync(Guid id);
}