using System.ComponentModel.DataAnnotations;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Responses.Appointments;
using Everwell.DAL.Data.Responses.User;

namespace Everwell.DAL.Data.Requests.STITests;

public class CreateSTITestRequest
{
    [Required] 
    public Guid AppointmentId { get; set; }

    [Required]
    public TestType TestType { get; set; }
        
    [Required]
    public Method Method { get; set; }

    [Required]
    public Status Status { get; set; } = Status.Pending;

    public DateOnly? CollectedDate { get; set; }
}