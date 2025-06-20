using System;

namespace Everwell.DAL.Data.Responses.Payment
{
    public class CreatePaymentResponse
    {
        public string PaymentUrl { get; set; }
        public Guid PaymentId { get; set; }
        public string PaymentMethod { get; set; } = "VNPay";
    }
} 