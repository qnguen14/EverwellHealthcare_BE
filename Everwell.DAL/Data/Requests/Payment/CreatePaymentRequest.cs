using System;
using System.ComponentModel.DataAnnotations;

namespace Everwell.DAL.Data.Requests.Payment
{
    public class CreatePaymentRequest
    {
        [Required]
        public Guid StiTestingId { get; set; }
        
        public string? PaymentMethod { get; set; } = "VNPay"; // VNPay, MoMo, ZaloPay, etc.
    }
} 