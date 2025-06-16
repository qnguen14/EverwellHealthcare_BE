// Create: ../Everwell.DAL/Data/Requests/MenstrualCycle/CreateMenstrualCycleRequest.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace Everwell.DAL.Data.Requests.MenstrualCycle
{
    public class CreateMenstrualCycleRequest
    {
        [Required(ErrorMessage = "Cycle start date is required")]
        [DataType(DataType.Date)]
        public DateTime CycleStartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? CycleEndDate { get; set; }

        [MaxLength(500, ErrorMessage = "Symptoms cannot be more than 500 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s,.-]*$", ErrorMessage = "Symptoms contain invalid characters")]
        public string? Symptoms { get; set; }

        [MaxLength(1000, ErrorMessage = "Notes cannot be more than 1000 characters")]
        public string? Notes { get; set; }

        [Range(1, 7, ErrorMessage = "Notify before days must be between 1 and 7")]
        public int? NotifyBeforeDays { get; set; }

        public bool NotificationEnabled { get; set; } = false;

        // Custom validation method
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // Validate cycle end date is after start date
            if (CycleEndDate.HasValue && CycleEndDate <= CycleStartDate)
            {
                results.Add(new ValidationResult("Cycle end date must be after start date", new[] { nameof(CycleEndDate) }));
            }

            // Validate period length
            if (CycleEndDate.HasValue)
            {
                var periodLength = (CycleEndDate.Value - CycleStartDate).TotalDays;
                if (periodLength > 10)
                {
                    results.Add(new ValidationResult("Period length cannot exceed 10 days", new[] { nameof(CycleEndDate) }));
                }
            }

            // Validate start date is not in future
            if (CycleStartDate > DateTime.UtcNow.Date)
            {
                results.Add(new ValidationResult("Cycle start date cannot be in the future", new[] { nameof(CycleStartDate) }));
            }

            // Validate notification settings
            if (NotificationEnabled && (!NotifyBeforeDays.HasValue || NotifyBeforeDays <= 0))
            {
                results.Add(new ValidationResult("Notify before days must be specified when notifications are enabled", new[] { nameof(NotifyBeforeDays) }));
            }

            return results;
        }
    }

    public class UpdateMenstrualCycleRequest
    {
        [Required(ErrorMessage = "Cycle start date is required")]
        public DateTime CycleStartDate { get; set; }

        public DateTime? CycleEndDate { get; set; }

        [MaxLength(1000, ErrorMessage = "Symptoms cannot be more than 1000 characters")]
        public string? Symptoms { get; set; }

        [MaxLength(2000, ErrorMessage = "Notes cannot be more than 2000 characters")]
        public string? Notes { get; set; }

        [Range(1, 7, ErrorMessage = "Notify before days must be between 1 and 7")]
        public int? NotifyBeforeDays { get; set; }

        public bool NotificationEnabled { get; set; } = false;
    }

    public class NotificationPreferencesRequest
    {
        public bool EnablePeriodReminders { get; set; }
        public bool EnableOvulationReminders { get; set; }
        public bool EnableFertilityReminders { get; set; }
        public bool EnableContraceptiveReminders { get; set; }

        [Range(1, 7, ErrorMessage = "Period reminder days must be between 1 and 7")]
        public int PeriodReminderDays { get; set; } = 2;

        [Range(1, 3, ErrorMessage = "Ovulation reminder days must be between 1 and 3")]
        public int OvulationReminderDays { get; set; } = 1;

        public List<TimeOnly> NotificationTimes { get; set; } = new();
    }

    // Custom validation result class with different name
    public class CycleValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}