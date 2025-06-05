using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Everwell.BLL.Services.Implements;

public class AppointmentService : BaseService<AppointmentService>, IAppointmentService
{
    public AppointmentService(IUnitOfWork<EverwellDbContext> unitOfWork, ILogger<AppointmentService> logger, IMapper mapper)
        : base(unitOfWork, logger, mapper)
    {
    }

    public async Task<IEnumerable<Appointment>> GetAllAppointmentsAsync()
    {
        try
        {
            var appointments = await _unitOfWork.GetRepository<Appointment>()
                .GetListAsync(
                    include: a => a.Include(ap => ap.Customer)
                                  .Include(ap => ap.Consultant)
                                  .Include(ap => ap.Service));
            
            return appointments ?? new List<Appointment>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all appointments");
            throw;
        }
    }

    public async Task<Appointment?> GetAppointmentByIdAsync(Guid id)
    {
        try
        {
            return await _unitOfWork.GetRepository<Appointment>()
                .FirstOrDefaultAsync(
                    predicate: a => a.AppointmentId == id,
                    include: a => a.Include(ap => ap.Customer)
                                  .Include(ap => ap.Consultant)
                                  .Include(ap => ap.Service));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting appointment by id: {Id}", id);
            throw;
        }
    }

    public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                appointment.AppointmentId = Guid.NewGuid();
                appointment.CreatedAt = DateTime.UtcNow;
                
                await _unitOfWork.GetRepository<Appointment>().InsertAsync(appointment);
                return appointment;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating appointment");
            throw;
        }
    }

    public async Task<Appointment?> UpdateAppointmentAsync(Guid id, Appointment appointment)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var existingAppointment = await _unitOfWork.GetRepository<Appointment>()
                    .FirstOrDefaultAsync(predicate: a => a.AppointmentId == id);
                
                if (existingAppointment == null) return null;

                existingAppointment.AppointmentDate = appointment.AppointmentDate;
                existingAppointment.Status = appointment.Status;
                existingAppointment.Notes = appointment.Notes;
                
                _unitOfWork.GetRepository<Appointment>().UpdateAsync(existingAppointment);
                return existingAppointment;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating appointment with id: {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAppointmentAsync(Guid id)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var appointment = await _unitOfWork.GetRepository<Appointment>()
                    .FirstOrDefaultAsync(predicate: a => a.AppointmentId == id);
                
                if (appointment == null) return false;

                _unitOfWork.GetRepository<Appointment>().DeleteAsync(appointment);
                return true;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting appointment with id: {Id}", id);
            throw;
        }
    }
} 