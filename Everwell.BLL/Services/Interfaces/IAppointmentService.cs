 using Everwell.DAL.Data.Entities;

namespace Everwell.BLL.Services.Interfaces;

public interface IAppointmentService
{
    Task<IEnumerable<Appointment>> GetAllAppointmentsAsync();
    Task<Appointment?> GetAppointmentByIdAsync(Guid id);
    Task<Appointment> CreateAppointmentAsync(Appointment appointment);
    Task<Appointment?> UpdateAppointmentAsync(Guid id, Appointment appointment);
    Task<bool> DeleteAppointmentAsync(Guid id);
}