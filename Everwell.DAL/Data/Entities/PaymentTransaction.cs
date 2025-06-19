using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Everwell.DAL.Data.Entities
{
    public enum PaymentStatus
    {
        Pending,
        Success,
        Failed,
        Cancelled
    }

    [Table("PaymentTransactions")]
    public class PaymentTransaction
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid StiTestingId { get; set; }

        [ForeignKey("StiTestingId")]
        public virtual STITesting StiTesting { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [Required]
        public string PaymentMethod { get; set; } = "VNPay";

        public string? TransactionId { get; set; } 

        public string? OrderInfo { get; set; }

        public string? ResponseCode { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}