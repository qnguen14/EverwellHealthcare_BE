using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Requests.Payment;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Everwell.API.Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("create-payment")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            try
            {
                var response = await _paymentService.CreatePaymentUrl(request, HttpContext);
                return Ok(response);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("vnpay-ipn")]
        public async Task<IActionResult> VnPayIpn()
        {
            var ipnResponse = await _paymentService.ProcessIpnResponse(Request.Query);
            // Returning the response required by VNPay
            return Ok(ipnResponse);
        }
    }
} 