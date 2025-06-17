using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Responses.STITests;
using Everwell.DAL.Data.Responses.User;

namespace Everwell.DAL.Data.Responses.TestResult
{
    public class CreateTestResultResponse
    {
        public Guid STITestingId { get; set; }
        public CreateSTITestResponse STITesting { get; set; }

        public string ResultData { get; set; }

        public TestResultStatus Status { get; set; } = TestResultStatus.Pending;

        public Guid? StaffId { get; set; }
        public GetUserResponse Staff { get; set; }

        public DateTime? ExaminedAt { get; set; }

        public DateTime? SentAt { get; set; }
    }
}
