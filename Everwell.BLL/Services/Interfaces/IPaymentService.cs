using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Requests.Payment;
using Everwell.DAL.Data.Responses.Payment;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Everwell.BLL.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<CreatePaymentResponse> CreatePaymentUrl(CreatePaymentRequest request, HttpContext context);
        Task<PaymentIpnResponse> ProcessIpnResponse(IQueryCollection vnpayData);
        Task<PaymentTransaction> GetPaymentTransaction(Guid transactionId);
        
        // New methods for payment history
        Task<CustomerPaymentHistoryResponse> GetCustomerPaymentHistory(Guid customerId);
        Task<List<PaymentHistoryResponse>> GetAllPaymentHistory(int page = 1, int pageSize = 20);
    }
}