using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Everwell.DAL.Data.Exceptions;
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
                                  .Include(ap => ap.Service),
                    orderBy: a => a.OrderBy(ap => ap.AppointmentDate));
            
            if (appointments != null && appointments.Any())
            {
                return _mapper.Map<IEnumerable<CreateAppointmentsResponse>>(appointments);
            }
            else
            {
                throw new NotFoundException("No appointments found");
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
                throw new NotFoundException($"Appointment with id {id} not found.");
            }
            
            return _mapper.Map<CreateAppointmentsResponse>(appointment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting appointment by id: {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<GetAppointmentConsultantResponse>> GetAppointmentsByConsultant(Guid id)
    {
        try
        {
            var appointments = await _unitOfWork.GetRepository<Appointment>()
                .GetListAsync(
                    predicate: a => a.ConsultantId == id &&
                                    a.Customer.IsActive &&
                                    a.Consultant.IsActive,
                    include: a => a.Include(ap => ap.Customer)
                        .Include(ap => ap.Consultant)
                        .Include(ap => ap.Service),
                    orderBy: a => a.OrderBy(ap => ap.AppointmentDate));

            if (appointments == null || !appointments.Any())
            {
                throw new NotFoundException($"No appointments found for consultant with id {id}");
            }

            return _mapper.Map<IEnumerable<GetAppointmentConsultantResponse>>(appointments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting appointments by consultant id: {Id}", id);
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
                    throw new BadRequestException("An appointment already exists for the specified date, slot, and consultant.");
                }
                
                var newAppointment = _mapper.Map<Appointment>(request);
                
                await _unitOfWork.GetRepository<Appointment>().InsertAsync(newAppointment);
                
                // Reload the appointment with navigation properties
                var insertedAppointment = await _unitOfWork.GetRepository<Appointment>()
                    .FirstOrDefaultAsync(
                        predicate: a => a.Id == newAppointment.Id,
                        include: a => a.Include(ap => ap.Customer)
                            .Include(ap => ap.Consultant)
                            .Include(ap => ap.Service)
                    );
                
                return _mapper.Map<CreateAppointmentsResponse>(insertedAppointment);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating appointment");
            throw;
        }
    }

    public async Task<CreateAppointmentsResponse> UpdateAppointmentAsync(Guid id, Appointment appointment)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var existingAppointment = await _unitOfWork.GetRepository<Appointment>()
                    .FirstOrDefaultAsync(predicate: a => a.Id == id);

                if (existingAppointment == null)
                {
                    throw new NotFoundException($"Appointment with ID {id} not found");
                }

                existingAppointment.AppointmentDate = appointment.AppointmentDate;
                existingAppointment.Status = appointment.Status;
                existingAppointment.Notes = appointment.Notes;
                
                _unitOfWork.GetRepository<Appointment>().UpdateAsync(existingAppointment);
                
                return _mapper.Map<CreateAppointmentsResponse>(existingAppointment);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating appointment with id: {Id}", id);
            throw;
        }
    }

    public async Task<DeleteAppointmentResponse> DeleteAppointmentAsync(Guid id)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var appointment = await _unitOfWork.GetRepository<Appointment>()
                    .FirstOrDefaultAsync(predicate: a => a.Id == id);
                
                if (appointment == null)
                {
                    throw new NotFoundException($"Appointment with ID {id} not found");
                };

                _unitOfWork.GetRepository<Appointment>().DeleteAsync(appointment);
                var response = _mapper.Map<DeleteAppointmentResponse>(appointment);
                response.IsDeleted = true;
                response.Message = "Appointment deleted successfully";
                return response;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting appointment with id: {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<GetScheduleResponse>> GetConsultantSchedules()
    {
        try
        {
            var schedules = await _unitOfWork.GetRepository<ConsultantSchedule>()
                .GetListAsync(
                    predicate: s => s.Consultant.IsActive,
                    include: s => s.Include(sc => sc.Consultant),
                    orderBy: s => s.OrderBy(sc => sc.WorkDate)
            );

            if (schedules == null)
            {
                throw new NotFoundException("No consultant schedules found");
            }

            return _mapper.Map<IEnumerable<GetScheduleResponse>>(schedules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting consultant schedules");
            throw;
        }
    }

    // Get Consultant Schedules by Their Id

    public async Task<IEnumerable<GetScheduleResponse>> GetConsultantSchedulesById(Guid id)
    {
        try
        {
            var schedules = await _unitOfWork.GetRepository<ConsultantSchedule>()
                .GetListAsync(
                    predicate: s => s.ConsultantId == id 
                                    && s.Consultant.IsActive,
                    include: s => s.Include(sc => sc.Consultant),
                    orderBy: s => s.OrderBy(sc => sc.WorkDate)
            );

            if (schedules == null)
            {
                throw new NotFoundException("No consultant schedules found");
            }

            return _mapper.Map<IEnumerable<GetScheduleResponse>>(schedules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occurred while getting consultant schedules with {id}");
            throw;
        }
    }

    public async Task<GetScheduleResponse> CreateConsultantSchedule(CreateScheduleRequest request)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var existingSchedule = await _unitOfWork.GetRepository<ConsultantSchedule>()
                    .FirstOrDefaultAsync(
                        predicate: s => s.ConsultantId == request.ConsultantId
                                        && s.WorkDate == request.WorkDate
                                        && s.Slot == request.Slot,
                        include: s => s.Include(sc => sc.Consultant));

                if (existingSchedule != null)
                {
                    throw new BadRequestException("This schedule already exists for this consultant.");
                }

                var newSchedule = _mapper.Map<ConsultantSchedule>(request);

                // Ensure values are set properly
                newSchedule.IsAvailable = request.IsAvailable;
                newSchedule.CreatedAt = DateTime.UtcNow;

                await _unitOfWork.GetRepository<ConsultantSchedule>().InsertAsync(newSchedule);

                return _mapper.Map<GetScheduleResponse>(newSchedule);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating consultant schedule: {@Request}", request);
            throw;
        }
    }
} 