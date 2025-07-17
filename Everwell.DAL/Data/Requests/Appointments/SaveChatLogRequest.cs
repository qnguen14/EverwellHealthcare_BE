using System;
using System.ComponentModel.DataAnnotations;

namespace Everwell.DAL.Data.Requests.Appointments;

public class SaveChatLogRequest
{
    [Required]
    public Guid AppointmentId { get; set; }

    [Required]
    public string LogContent { get; set; }
}
