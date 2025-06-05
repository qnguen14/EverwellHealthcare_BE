using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Everwell.DAL.Data.Requests.Appointments;
using Everwell.DAL.Data.Responses.Appointments;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Everwell.BLL.Services.Implements;

public class AppointmentService : BaseService<AppointmentService>, IAppointmentService
{
    public AppointmentService(IUnitOfWork<EverwellDbContext> unitOfWork, ILogger<AppointmentService> logger, IMapper mapper)
        : base(unitOfWork, logger, mapper)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CreateAppointmentsResponse>> GetAllAppointmentsAsync()
    {
        try
        {
            var appointments = await _unitOfWork.GetRepository<Appointment>()
                .GetListAsync(
                    predicate: a => a.Customer.IsActive == true 
                                    && a.Consultant.IsActive == true,
                    include: a => a.Include(ap => ap.Customer)
                                  .Include(ap => ap.Consultant)
                                  .Include(ap => ap.Service));
            
            if (appointments != null && appointments.Any())
            {
                return _mapper.Map<IEnumerable<CreateAppointmentsResponse>>(appointments);
            }
            else
            {
                throw new DirectoryNotFoundException("No appointments found");
            }
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all appointments");
            throw;
        }
    }

    public async Task<CreateAppointmentsResponse> GetAppointmentByIdAsync(Guid id)
    {
        try
        {
            var appointment = await _unitOfWork.GetRepository<Appointment>()
                .FirstOrDefaultAsync(
                    predicate: a => a.Id == id 
                                    && a.Customer.IsActive == true 
                                    && a.Consultant.IsActive == true,
                    include: a => a.Include(ap => ap.Customer)
                                  .Include(ap => ap.Consultant)
                                  .Include(ap => ap.Service));
            
            if (appointment == null)
            {
                throw new KeyNotFoundException($"Appointment with id {id} not found.");
            }
            
            return _mapper.Map<CreateAppointmentsResponse>(appointment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting appointment by id: {Id}", id);
            throw;
        }
    }

    public async Task<CreateAppointmentsResponse> CreateAppointmentAsync(CreateAppointmentRequest request)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request), "request cannot be null.");
                }
                
                var existingAppointment = await _unitOfWork.GetRepository<Appointment>()
                    .FirstOrDefaultAsync(predicate: a => a.AppointmentDate == request.AppointmentDate 
                                                         && a.Slot == request.Slot 
                                                         && a.ConsultantId == request.ConsultantId,
                                        include: a => a.Include(ap => ap.Customer)
                                            .Include(ap => ap.Consultant)
                                                         );
                if (existingAppointment != null &&
                    existingAppointment.Customer.IsActive &&
                    existingAppointment.Consultant.IsActive)
                {
                    throw new InvalidOperationException("An appointment already exists for the specified date, slot, and consultant.");
                }
                
                var newAppointment = _mapper.Map<Appointment>(request);
                
                await _unitOfWork.GetRepository<Appointment>().InsertAsync(newAppointment);
                
                return _mapper.Map<CreateAppointmentsResponse>(newAppointment);
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
                    .FirstOrDefaultAsync(predicate: a => a.Id == id);
                
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
                    .FirstOrDefaultAsync(predicate: a => a.Id == id);
                
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