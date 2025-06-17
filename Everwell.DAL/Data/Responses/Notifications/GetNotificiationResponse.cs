using Everwell.DAL.Data.Responses.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System;

namespace Everwell.DAL.Data.Responses.Notifications
{
    public class GetNotificationResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public string Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public Guid? AppointmentId { get; set; }
        public Guid? TestResultId { get; set; }
        public Guid? STITestingId { get; set; }
    }
}