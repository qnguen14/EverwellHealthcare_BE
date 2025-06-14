using Everwell.DAL.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everwell.DAL.Data.Requests.TestResult
{
    public class CreateTestResultRequest
    {
        [Required(ErrorMessage = "STITestingId is required")]
        public Guid STITestingId { get; set; }

        [Required(ErrorMessage = "Result Data is required")]
        public string ResultData { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public TestResultStatus Status { get; set; } = TestResultStatus.Pending;

        [Required(ErrorMessage = "Customer Id is required")]
        public Guid? CustomerId { get; set; }

        [Required(ErrorMessage = "Staff Id is required")]
        public Guid? StaffId { get; set; }

        [Required(ErrorMessage = "Examined At is required")]
        public DateTime? ExaminedAt { get; set; }

        [Required(ErrorMessage = "Sent At is required")]
        public DateTime? SentAt { get; set; }
    }
}
