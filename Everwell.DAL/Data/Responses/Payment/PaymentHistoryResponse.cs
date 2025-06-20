using System;
using System.Collections.Generic;
using Everwell.DAL.Data.Entities;

namespace Everwell.DAL.Data.Responses.Payment
{
    public class PaymentHistoryResponse
    {
        public Guid TransactionId { get; set; }
        public Guid StiTestingId { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public string PaymentMethod { get; set; }
        public string? TransactionReference { get; set; }
        public string? OrderInfo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // STI Testing details for context
        public string TestPackage { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public string TestingStatus { get; set; }
    }

    public class CustomerPaymentHistoryResponse
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalSuccessfulAmount { get; set; }
        public List<PaymentHistoryResponse> PaymentHistory { get; set; } = new();
    }
} 