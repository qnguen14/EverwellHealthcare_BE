// Create: ../Everwell.DAL/Data/Requests/MenstrualCycle/CreateMenstrualCycleRequest.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace Everwell.DAL.Data.Requests.MenstrualCycle
{
    public class CreateMenstrualCycleRequest
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
}