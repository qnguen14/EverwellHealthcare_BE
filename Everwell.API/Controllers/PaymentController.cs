using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Requests.Payment;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

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
                return Ok(new { is_success = true, data = response, message = "Payment URL created successfully" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { is_success = false, message = ex.Message });
            }
        }

        [HttpGet("vnpay-ipn")]
        public async Task<IActionResult> VnPayIpn()
        {
            var ipnResponse = await _paymentService.ProcessIpnResponse(Request.Query);
            // Returning the response required by VNPay
            return Ok(ipnResponse);
        }

        [HttpPost("vnpay-callback")]
        public async Task<IActionResult> ProcessVnPayCallback([FromBody] Dictionary<string, string> vnpayParams)
        {
            try
            {
                // Convert dictionary to query collection for processing
                var queryCollection = new Microsoft.AspNetCore.Http.QueryCollection(
                    vnpayParams.ToDictionary(kvp => kvp.Key, kvp => new Microsoft.Extensions.Primitives.StringValues(kvp.Value))
                );

                var ipnResponse = await _paymentService.ProcessIpnResponse(queryCollection);
                
                if (ipnResponse.RspCode == "00")
                {
                    return Ok(new { 
                        is_success = true, 
                        rspCode = ipnResponse.RspCode,
                        message = "Payment processed successfully" 
                    });
                }
                else
                {
                    return BadRequest(new { 
                        is_success = false, 
                        rspCode = ipnResponse.RspCode,
                        message = ipnResponse.Message 
                    });
                }
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { 
                    is_success = false, 
                    rspCode = "99",
                    message = ex.Message 
                });
            }
        }

        [HttpGet("transaction/{transactionId}")]
        public async Task<IActionResult> GetPaymentTransaction(Guid transactionId)
        {
            try
            {
                var transaction = await _paymentService.GetPaymentTransaction(transactionId);
                if (transaction == null)
                {
                    return NotFound(new { is_success = false, message = "Transaction not found" });
                }

                return Ok(new { is_success = true, data = transaction, message = "Transaction retrieved successfully" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { is_success = false, message = ex.Message });
            }
        }
    }
} 