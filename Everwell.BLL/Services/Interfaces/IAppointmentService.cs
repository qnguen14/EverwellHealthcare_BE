using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Requests.Appointments;
using Everwell.DAL.Data.Responses.Appointments;

namespace Everwell.BLL.Services.Interfaces;

public interface IAppointmentService
{
    Task<IEnumerable<CreateAppointmentsResponse>> GetAllAppointmentsAsync();
    Task<CreateAppointmentsResponse> GetAppointmentByIdAsync(Guid id); // to do: getappointmentresponse
    Task<IEnumerable<GetAppointmentConsultantResponse>> GetAppointmentsByConsultant (Guid id); // to do: getappointmentresponse
    Task<CreateAppointmentsResponse> CreateAppointmentAsync(CreateAppointmentRequest request);
    Task<CreateAppointmentsResponse> UpdateAppointmentAsync(Guid id, UpdateAppointmentRequest request);
    Task<CreateAppointmentsResponse> CancelAppoinemntAsync(Guid id);
    Task<DeleteAppointmentResponse> DeleteAppointmentAsync(Guid id);
    Task<IEnumerable<GetScheduleResponse>> GetConsultantSchedules();
    Task<IEnumerable<GetScheduleResponse>> GetConsultantSchedulesById(Guid id);
    Task<GetScheduleResponse> CreateConsultantSchedule(CreateScheduleRequest request);
}