﻿using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Responses.User;

namespace Everwell.DAL.Data.Responses.Appointments;

public class CreateAppointmentsResponse
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }
    public GetUserResponse Customer { get; set; }

    public Guid ConsultantId { get; set; }
    public GetUserResponse Consultant { get; set; }

    // public Guid ServiceId { get; set; } // to do: GetServiceResponse

    public DateOnly AppointmentDate { get; set; }

    public ShiftSlot? Slot { get; set; }
    
    public AppointmentStatus Status { get; set; }

    public string? Notes { get; set; }
    
    public string? GoogleMeetLink { get; set; }
    
    public string? GoogleEventId { get; set; }
    
    public string? MeetingId { get; set; }
    
    public bool IsVirtual { get; set; }

    public DateTime CreatedAt { get; set; }

    // Thêm thời gian check-in / check-out
    public DateTime? CheckInTimeUtc { get; set; }
    public DateTime? CheckOutTimeUtc { get; set; }
}