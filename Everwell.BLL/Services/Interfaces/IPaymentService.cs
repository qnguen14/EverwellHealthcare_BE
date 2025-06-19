using Everwell.DAL.Data.Requests.Payment;
using Everwell.DAL.Data.Responses.Payment;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Everwell.BLL.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<CreatePaymentResponse> CreatePaymentUrl(CreatePaymentRequest request, HttpContext context);
        Task<PaymentIpnResponse> ProcessIpnResponse(IQueryCollection vnpayData);
    }
}