using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Everwell.DAL.Data.Exceptions;
using Everwell.DAL.Data.Requests.Appointments;
using Everwell.DAL.Data.Responses.Appointments;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Everwell.BLL.Services.Implements;

public class AppointmentService : BaseService<AppointmentService>, IAppointmentService
{
    public AppointmentService(IUnitOfWork<EverwellDbContext> unitOfWork, ILogger<AppointmentService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
    }

    #region Helper method to create appointment notifications

    private string GetReadableTimeSlot(ShiftSlot slot)
    {
        return slot switch
        {
            ShiftSlot.Morning1 => "8:00 - 10:00",
            ShiftSlot.Morning2 => "10:00 - 12:00",
            ShiftSlot.Afternoon1 => "13:00 - 15:00",
            ShiftSlot.Afternoon2 => "15:00 - 17:00",
            _ => slot.ToString()
        };
    }
    
    private async Task CreateAppointmentNotification(Appointment appointment, string title, string message, NotificationPriority priority = NotificationPriority.Medium)
    {
        try
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = appointment.CustomerId,
                Title = title,
                Message = message,
                Type = NotificationType.Appointment,
                Priority = priority,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                AppointmentId = appointment.Id
            };

            await _unitOfWork.GetRepository<Notification>().InsertAsync(notification);
            _logger.LogInformation("Created appointment notification for user {UserId}, appointment {AppointmentId}",
                appointment.CustomerId, appointment.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create appointment notification for appointment {AppointmentId}", appointment.Id);
            // Don't throw - notification creation shouldn't block the main operation
        }
    }
    
    
    #endregion 

    public async Task<IEnumerable<CreateAppointmentsResponse>> GetAllAppointmentsAsync()
    {
        try
        {
            var appointments = await _unitOfWork.GetRepository<Appointment>()
                .GetListAsync(
                    predicate: a => a.Customer.IsActive == true 
                                    && a.Consultant.IsActive == true,
                    include: a => a.Include(ap => ap.Customer)
                                  .Include(ap => ap.Consultant),
                                  // .Include(ap => ap.Service),
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
                                  .Include(ap => ap.Consultant));
                                  // .Include(ap => ap.Service));
            
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
                        .Include(ap => ap.Consultant),
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
                throw new ArgumentNullException(nameof(request), "request cannot be null.");

            var existingAppointment = await _unitOfWork.GetRepository<Appointment>()
                .FirstOrDefaultAsync(
                    predicate: a => a.AppointmentDate == request.AppointmentDate
                                    && a.Slot == request.Slot
                                    && a.ConsultantId == request.ConsultantId,
                    include: a => a.Include(ap => ap.Customer)
                                   .Include(ap => ap.Consultant)
                );
            if (existingAppointment != null &&
                existingAppointment.Customer.IsActive &&
                existingAppointment.Consultant.IsActive)
            {
                throw new BadRequestException(
                    "An appointment already exists for the specified date, slot, and consultant.");
            }

            var newAppointment = _mapper.Map<Appointment>(request);
            if (newAppointment.Id == Guid.Empty)
                newAppointment.Id = Guid.NewGuid(); // Ensure Id is set

            await _unitOfWork.GetRepository<Appointment>().InsertAsync(newAppointment);

            await CreateAppointmentNotification(newAppointment,
                "Cuộc hẹn đã được đặt",
                $"Cuộc hẹn của bạn với {newAppointment.Consultant?.Name} " +
                $"vào ngày {newAppointment.AppointmentDate} " +
                $"lúc {GetReadableTimeSlot(newAppointment.Slot)} đã được đặt thành công.");

            return _mapper.Map<CreateAppointmentsResponse>(newAppointment);
        });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating appointment");
            throw;
        }
    }

    public async Task<CreateAppointmentsResponse> UpdateAppointmentAsync(Guid id, UpdateAppointmentRequest request)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var existingAppointment = await _unitOfWork.GetRepository<Appointment>()
                    .FirstOrDefaultAsync(
                        predicate: a => a.Id == id 
                                        && a.Customer.IsActive == true 
                                        && a.Consultant.IsActive == true,
                        include: a => a.Include(ap => ap.Customer)
                            .Include(ap => ap.Consultant));
                            // .Include(ap => ap.Service));

                if (existingAppointment == null)
                {
                    throw new NotFoundException($"Appointment with ID {id} not found");
                }

                existingAppointment.AppointmentDate = request.AppointmentDate;
                existingAppointment.Status = request.Status;
                existingAppointment.Notes = request.Notes;
                
                _unitOfWork.GetRepository<Appointment>().UpdateAsync(existingAppointment);
                

                await CreateAppointmentNotification(existingAppointment, 
                    "Cuộc hẹn đã được cập nhật", 
                    $"Cuộc hẹn của bạn với {existingAppointment.Consultant.Name} " +
                    $"vào ngày {existingAppointment.AppointmentDate} " +
                    $"lúc {GetReadableTimeSlot(existingAppointment.Slot)} đã đuợc cập nhật.");

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
                    .FirstOrDefaultAsync(
                        predicate: a => a.Id == id
                                        && a.Customer.IsActive == true
                                        && a.Consultant.IsActive == true,
                        include: a => a.Include(ap => ap.Customer)
                            .Include(ap => ap.Consultant));
                            // .Include(ap => ap.Service));
                
                if (appointment == null)
                {
                    throw new NotFoundException($"Appointment with ID {id} not found");
                };

                var response = _mapper.Map<DeleteAppointmentResponse>(appointment);
                response.IsDeleted = true;
                response.Message = "Appointment deleted successfully";
                
                await CreateAppointmentNotification(appointment, 
                    "Appointment Cancelled", 
                    $"Your appointment with {appointment.Consultant.Name} " +
                    $"on {appointment.AppointmentDate} " +
                    $"at {appointment.Slot} has been cancelled.");
                
                _unitOfWork.GetRepository<Appointment>().DeleteAsync(appointment);

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

    public async Task<CreateAppointmentsResponse> CancelAppoinemntAsync(Guid id)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var appointment = await _unitOfWork.GetRepository<Appointment>()
                    .FirstOrDefaultAsync(
                        predicate: a => a.Id == id 
                                        && a.Customer.IsActive == true 
                                        && a.Consultant.IsActive == true,
                        include: a => a.Include(ap => ap.Customer)
                            .Include(ap => ap.Consultant));
                if (appointment == null)
                {
                    throw new NotFoundException($"Appointment with ID {id} not found");
                }
                appointment.Status = AppointmentStatus.Cancelled;
                
                _unitOfWork.GetRepository<Appointment>().UpdateAsync(appointment);
                await CreateAppointmentNotification(appointment, 
                    "Appointment Cancelled", 
                    $"Your appointment with {appointment.Consultant.Name} " +
                    $"on {appointment.AppointmentDate} " +
                    $"at {GetReadableTimeSlot(appointment.Slot)} has been cancelled.");
                return _mapper.Map<CreateAppointmentsResponse>(appointment);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while cancelling appointment with id: {Id}", id);
            throw;
        }
    }
} 