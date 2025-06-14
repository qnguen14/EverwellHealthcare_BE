using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Responses.Appointments;
using Everwell.DAL.Data.Responses.User;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everwell.DAL.Data.Responses.STITests
{
    public class CreateSTITestResponse
    {
        public Guid AppointmentId { get; set; }
        public CreateAppointmentsResponse Appointment { get; set; }

        public Guid CustomerId { get; set; }
        public GetUserResponse Customer { get; set; }

        public TestType TestType { get; set; }

        public Method Method { get; set; }

        public Status Status { get; set; } 

        public DateOnly? CollectedDate { get; set; }
    }
}
