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

    private bool IsValidDate(DateOnly date)
    {
        // Check if the requested date is in the future
        return date > DateOnly.FromDateTime(DateTime.UtcNow) ? true : false;
    }
    
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
                
                            if (!IsValidDate(request.AppointmentDate))
                            {
                                _logger.LogWarning("Invalid appointment date: {AppointmentDate}", request.AppointmentDate);
                                return null;
                            }
                
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
                            
                            // Create Agora channel if appointment is virtual
                            if (request.IsVirtual)
                            {
                                try
                                {
                                    _logger.LogInformation("🔍 Creating Agora meeting for virtual appointment {AppointmentId}", newAppointment.Id);
                                    var meetLink = await _calendarService.CreateVideoMeetingAsync(newAppointment);
                                    
                                    if (string.IsNullOrEmpty(meetLink))
                                    {
                                        throw new Exception("Meeting link generation returned null or empty");
                                    }
                                    
                                    newAppointment.GoogleMeetLink = meetLink; // Store Agora meeting URL
                                    newAppointment.MeetingId = ExtractRoomNameFromUrl(meetLink); // Store channel name
                                    
                                    _logger.LogInformation("✅ Agora channel created successfully for appointment {AppointmentId}: {MeetLink}", 
                                        newAppointment.Id, meetLink);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "❌ Failed to create Agora channel for appointment {AppointmentId}. Appointment will remain virtual but without meeting link.", newAppointment.Id);
                                    _logger.LogError("🔍 DEBUG - Agora error details: {ErrorMessage}", ex.Message);
                                    _logger.LogError("🔍 DEBUG - Agora stack trace: {StackTrace}", ex.StackTrace);
                                    
                                    // Set a fallback meeting URL so users can still access the meeting page
                                    var fallbackUrl = $"{_configuration?["Agora:BaseUrl"] ?? "http://localhost:5173/meeting"}/{newAppointment.Id}";
                                    newAppointment.GoogleMeetLink = fallbackUrl;
                                    newAppointment.MeetingId = newAppointment.Id.ToString();
                                    
                                    _logger.LogWarning("⚠️ Set fallback meeting URL for appointment {AppointmentId}: {FallbackUrl}", 
                                        newAppointment.Id, fallbackUrl);
                                }
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
                if (!IsValidDate(request.AppointmentDate))
                {
                    _logger.LogWarning("Invalid appointment date: {AppointmentDate}", request.AppointmentDate);
                    return null;
                }
                
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
                
                // If changing from non-virtual to virtual, create Agora channel
                if (!wasVirtual && request.IsVirtual)
                {
                    try
                    {
                        var meetLink = await _calendarService.CreateVideoMeetingAsync(existingAppointment);
                        existingAppointment.GoogleMeetLink = meetLink;
                        
                        _logger.LogInformation("Agora channel created for updated appointment {AppointmentId}: {MeetLink}", 
                            existingAppointment.Id, meetLink);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create Agora channel for updated appointment {AppointmentId}. Appointment will remain virtual but without meeting link.", existingAppointment.Id);
                        // Keep the appointment as virtual even if video meeting creation fails
                    }
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
} 