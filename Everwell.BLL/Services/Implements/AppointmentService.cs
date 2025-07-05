using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using DocumentFormat.OpenXml.Drawing.Charts;
using Everwell.DAL.Data.Exceptions;
using Everwell.DAL.Data.Requests.Appointments;
using Everwell.DAL.Data.Responses.Appointments;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Everwell.BLL.Services.Implements;

public class AppointmentService : BaseService<AppointmentService>, IAppointmentService
{
    private readonly ICalendarService _calendarService;
    private readonly IConfiguration _configuration;

    public AppointmentService(IUnitOfWork<EverwellDbContext> unitOfWork, ILogger<AppointmentService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, ICalendarService calendarService, IConfiguration configuration)
        : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
        _calendarService = calendarService;
        _configuration = configuration;
    }

    #region Helper methods

    //private bool IsValidDate(DateOnly date, ShiftSlot slot)
    //{
    //    /*
    //     * A date is considered valid when:
    //     *   • It is strictly in the future, OR
    //     *   • It is today and the selected slot's start-time has not yet passed.
    //     */
    //    var today = DateOnly.FromDateTime(DateTime.UtcNow);

    //    // Future days are always valid
    //    if (date > today)
    //        return true;

    //    // Past days are invalid
    //    if (date < today)
    //        return false;

    //    // Same-day booking – ensure the slot has not started yet
    //    int startHour = slot switch
    //    {
    //        ShiftSlot.Morning1 => 8,
    //        ShiftSlot.Morning2 => 10,
    //        ShiftSlot.Afternoon1 => 13,
    //        ShiftSlot.Afternoon2 => 15,
    //        _ => 0
    //    };

    //    var slotStart = date.ToDateTime(new TimeOnly(startHour, 0));
    //    return DateTime.UtcNow < slotStart;
    //}
    
    private bool IsCancelled(Appointment appointment)
    {
        // Check if the appointment is cancelled
        return appointment.Status == AppointmentStatus.Cancelled;
    }
    
    private string ExtractRoomNameFromUrl(string url)
    {
        try
        {
            if (string.IsNullOrEmpty(url)) return null;
            
            var uri = new Uri(url);
            
            return null;
        }
        catch
        {
            return null;
        }
    }
    
    private bool IsValidVideoMeetingUrl(string url)
    {
        try
        {
            // Check if the URL is not null
            if (string.IsNullOrEmpty(url))
                return false;

            // Create a Uri object to validate the URL
            var uri = new Uri(url);
        
            // Accept various video meeting platforms
            if (uri.Host.Equals("meet.jit.si", StringComparison.OrdinalIgnoreCase))
            {
                // Jitsi Meet - any room name is valid
                return !string.IsNullOrEmpty(uri.AbsolutePath.TrimStart('/'));
            }
            else if (uri.Host.Equals("meet.google.com", StringComparison.OrdinalIgnoreCase))
            {
                // Google Meet - check format
                var path = uri.AbsolutePath.TrimStart('/');
                return path.Count(c => c == '-') >= 2 && path.Length >= 9;
            }
            else if (uri.Host.Contains("zoom.us", StringComparison.OrdinalIgnoreCase) ||
                     uri.Host.Contains("teams.microsoft.com", StringComparison.OrdinalIgnoreCase))
            {
                // Zoom or Teams - basic URL validation
                return true;
            }
            
            return false;
        }
        catch
        {
            // If there's any exception while parsing the URL, it's not valid
            return false;
        }
    }

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

    private string GetReadableTimeSlot(Appointment appointment)
    {
        // Use ShiftSlot for time display
        return GetReadableTimeSlot(appointment.Slot);
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
                
                            //if (!IsValidDate(request.AppointmentDate, request.Slot))
                            //{
                            //    _logger.LogWarning("Invalid appointment date: {AppointmentDate}", request.AppointmentDate);
                            //    return null;
                            //}
                
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
                                existingAppointment.Consultant.IsActive &&
                                !IsCancelled(existingAppointment))
                            {
                                throw new BadRequestException(
                                    "An appointment already exists for the specified date, slot, and consultant.");
                            }
                
                            var newAppointment = _mapper.Map<Appointment>(request);
                            if (newAppointment.Id == Guid.Empty)
                                newAppointment.Id = Guid.NewGuid(); // Ensure Id is set
                
                            // Load customer and consultant information for video meeting
                            var customer = await _unitOfWork.GetRepository<User>().FirstOrDefaultAsync(
                                predicate: u => u.Id == request.CustomerId);
                            var consultant = await _unitOfWork.GetRepository<User>().FirstOrDefaultAsync(
                                predicate: u => u.Id == request.ConsultantId);
                            
                            newAppointment.Customer = customer;
                            newAppointment.Consultant = consultant;
                
                            // Set the virtual meeting flag as requested
                            newAppointment.IsVirtual = request.IsVirtual;
                            
                            // Create Daily room if appointment is virtual
                            if (request.IsVirtual)
                            {
                                // Set Agora meeting URL - no need to create room in advance
                                var agoraMeetingUrl = $"{_configuration?["Frontend:BaseUrl"] ?? "http://localhost:5173"}/meeting/{newAppointment.Id}";
                                newAppointment.GoogleMeetLink = agoraMeetingUrl;
                                newAppointment.MeetingId = newAppointment.Id.ToString();
                                
                                _logger.LogInformation("✅ Agora meeting URL set for appointment {AppointmentId}: {MeetingUrl}", 
                                    newAppointment.Id, agoraMeetingUrl);
                            }
                
                            await _unitOfWork.GetRepository<Appointment>().InsertAsync(newAppointment);
                
                            var notificationMessage = request.IsVirtual && !string.IsNullOrEmpty(newAppointment.GoogleMeetLink)
                                ? $"Cuộc hẹn trực tuyến của bạn với {newAppointment.Consultant?.Name} " +
                                  $"vào ngày {newAppointment.AppointmentDate} " +
                                  $"lúc {GetReadableTimeSlot(newAppointment)} đã được đặt thành công. " +
                                  $"Link video meeting: {newAppointment.GoogleMeetLink}"
                                : $"Cuộc hẹn của bạn với {newAppointment.Consultant?.Name} " +
                                  $"vào ngày {newAppointment.AppointmentDate} " +
                                  $"lúc {GetReadableTimeSlot(newAppointment)} đã được đặt thành công.";
                
                            await CreateAppointmentNotification(newAppointment,
                                "Cuộc hẹn đã được đặt",
                                notificationMessage);
                
                            return _mapper.Map<CreateAppointmentsResponse>(newAppointment);
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred while creating appointment");
                        throw;
                    }
                }

    public async Task<CreateAppointmentsResponse?> UpdateAppointmentAsync(Guid id, UpdateAppointmentRequest request)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                //if (!IsValidDate(request.AppointmentDate, request.Slot))
                //{
                //    _logger.LogWarning("Invalid appointment date: {AppointmentDate}", request.AppointmentDate);
                //    return null;
                //}
                
                var existingAppointment = await _unitOfWork.GetRepository<Appointment>()
                    .FirstOrDefaultAsync(
                        a => a.Id == id && a.Customer.IsActive && a.Consultant.IsActive,
                        null,
                        a => a.Include(ap => ap.Customer)
                            .Include(ap => ap.Consultant));

                if (existingAppointment == null)
                {
                    throw new NotFoundException($"Appointment with ID {id} not found");
                }
                
                var conflictingAppointment = await _unitOfWork.GetRepository<Appointment>()
                    .FirstOrDefaultAsync(
                        predicate: a => a.AppointmentDate == request.AppointmentDate
                                        && a.Slot == request.Slot
                                        && a.ConsultantId == existingAppointment.ConsultantId
                                        && a.Id != id
                                        && a.Status != AppointmentStatus.Cancelled,
                        include: a => a.Include(ap => ap.Customer)
                            .Include(ap => ap.Consultant)
                    );

                if (conflictingAppointment != null)
                {
                    throw new BadRequestException(
                        "An appointment already exists for the specified date, slot, and consultant.");
                }
                
                

                existingAppointment.AppointmentDate = request.AppointmentDate;
                existingAppointment.Status = request.Status;
                existingAppointment.Notes = request.Notes;
                
                // Handle virtual meeting changes
                bool wasVirtual = existingAppointment.IsVirtual;
                existingAppointment.IsVirtual = request.IsVirtual;
                
                // If changing from non-virtual to virtual, set Agora meeting URL
                if (!wasVirtual && request.IsVirtual)
                {
                    var agoraMeetingUrl = $"{_configuration?["Frontend:BaseUrl"] ?? "http://localhost:5173"}/meeting/{existingAppointment.Id}";
                    existingAppointment.GoogleMeetLink = agoraMeetingUrl;
                    existingAppointment.MeetingId = existingAppointment.Id.ToString();
                    
                    _logger.LogInformation("Agora meeting URL set for updated appointment {AppointmentId}: {MeetingUrl}", 
                        existingAppointment.Id, agoraMeetingUrl);
                }
                // If changing from virtual to non-virtual, remove video meeting
                else if (wasVirtual && !request.IsVirtual)
                {
                    if (!string.IsNullOrEmpty(existingAppointment.GoogleEventId))
                    {
                        try
                        {
                            await _calendarService.DeleteCalendarEventAsync(existingAppointment.GoogleEventId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to delete calendar event for appointment {AppointmentId}", existingAppointment.Id);
                        }
                    }
                    existingAppointment.GoogleMeetLink = null;
                    existingAppointment.GoogleEventId = null;
                    existingAppointment.MeetingId = null;
                }
                // If already virtual and still virtual, update the meeting
                else if (wasVirtual && request.IsVirtual && !string.IsNullOrEmpty(existingAppointment.GoogleEventId))
                {
                    try
                    {
                        await _calendarService.UpdateCalendarEventAsync(existingAppointment.GoogleEventId, existingAppointment);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to update calendar event for appointment {AppointmentId}", existingAppointment.Id);
                    }
                }
                
                _unitOfWork.GetRepository<Appointment>().UpdateAsync(existingAppointment);
                

                var updateMessage = existingAppointment.IsVirtual && !string.IsNullOrEmpty(existingAppointment.GoogleMeetLink)
                    ? $"Cuộc hẹn trực tuyến của bạn với {existingAppointment.Consultant.Name} " +
                      $"vào ngày {existingAppointment.AppointmentDate} " +
                      $"lúc {GetReadableTimeSlot(existingAppointment)} đã được cập nhật. " +
                      $"Link video meeting: {existingAppointment.GoogleMeetLink}"
                    : $"Cuộc hẹn của bạn với {existingAppointment.Consultant.Name} " +
                      $"vào ngày {existingAppointment.AppointmentDate} " +
                      $"lúc {GetReadableTimeSlot(existingAppointment)} đã được cập nhật.";

                await CreateAppointmentNotification(existingAppointment, 
                    "Cuộc hẹn đã được cập nhật", 
                    updateMessage);

                return _mapper.Map<CreateAppointmentsResponse>(existingAppointment);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating appointment with id: {Id}", id);
            throw;
        }
    }

    public async Task<CreateAppointmentsResponse> UpdateMeetingLinkAsync(Guid id, string meetingLink)
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
            
                if (string.IsNullOrWhiteSpace(meetingLink))
                {
                    throw new BadRequestException("Meeting link cannot be empty");
                }
            
                // Validate the meeting link format
                if (!IsValidVideoMeetingUrl(meetingLink))
                {
                    throw new BadRequestException("Invalid video meeting URL. URL must be from a supported platform (Jitsi Meet, Google Meet, Zoom, or Teams).");
                }

                existingAppointment.GoogleMeetLink = meetingLink;
            
                _unitOfWork.GetRepository<Appointment>().UpdateAsync(existingAppointment);
            

                await CreateAppointmentNotification(existingAppointment, 
                    "Cuộc hẹn đã được cập nhật", 
                    $"Cuộc hẹn của bạn với {existingAppointment.Consultant.Name} " +
                    $"vào ngày {existingAppointment.AppointmentDate} " +
                    $"lúc {GetReadableTimeSlot(existingAppointment)} đã được cập nhật đường dẫn Google Meet.");

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
                    $"at {GetReadableTimeSlot(appointment)} has been cancelled.");
                
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
                    $"at {GetReadableTimeSlot(appointment)} has been cancelled.");
                return _mapper.Map<CreateAppointmentsResponse>(appointment);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while cancelling appointment with id: {Id}", id);
            throw;
        }
    }

    #region Check-in / Check-out

    public async Task<CheckInResponse> MarkCheckInAsync(Guid id)
    {
        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var appt = await _unitOfWork.GetRepository<Appointment>()
                    .FirstOrDefaultAsync(
                        a => a.Id == id && a.Customer.IsActive && a.Consultant.IsActive,
                        null,
                        a => a.Include(ap => ap.Customer)
                              .Include(ap => ap.Consultant));
                if (appt == null) return null;
                // Check if the appointment is already checked in
                if (appt.CheckInTimeUtc.HasValue)
                {
                    _logger.LogWarning("Appointment {Id} is already checked in at {CheckInTime}", id, appt.CheckInTimeUtc);
                    return _mapper.Map<CheckInResponse>(appt);
                }
                // Mark check-in time
                appt.CheckInTimeUtc = DateTime.UtcNow;
                appt.Status = AppointmentStatus.Temp; // Set to Temp status for check-in
                
                await _unitOfWork.SaveChangesAsync();
                
                return _mapper.Map<CheckInResponse>(appt);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark check-in for appointment {Id}", id);
            throw;
        }
    }

    public async Task<CheckOutResponse> MarkCheckOutAsync(Guid id)
    {
        try
        {
            var appt = await _unitOfWork.GetRepository<Appointment>()
                .FirstOrDefaultAsync(
                    a => a.Id == id && a.Customer.IsActive && a.Consultant.IsActive,
                    null,
                    a => a.Include(ap => ap.Customer)
                        .Include(ap => ap.Consultant));
            
            if (appt == null) return null;
            
            if (appt.CheckOutTimeUtc.HasValue)
            {
                _logger.LogWarning("Appointment {Id} is already checked out at {CheckOutTime}", id, appt.CheckOutTimeUtc);
                return _mapper.Map<CheckOutResponse>(appt);
            }

            appt.CheckOutTimeUtc = DateTime.UtcNow;
            appt.Status = AppointmentStatus.Completed;
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CheckOutResponse>(appt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark check-out for appointment {Id}", id);
            throw;
        }
    }

    #endregion
} 