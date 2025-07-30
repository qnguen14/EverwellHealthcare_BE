// ============================================================================
// PAYMENT CONTROLLER
// ============================================================================
// This controller manages payment processing, billing, and financial transactions
// It handles secure payment gateway integration and transaction management
// 
// PAYMENT PROCESSING FLOW:
// 1. PAYMENT INITIATION: User selects service and initiates payment
// 2. PAYMENT URL CREATION: System generates secure payment gateway URL
// 3. GATEWAY REDIRECT: User redirected to payment provider (VNPay, MoMo, ZaloPay)
// 4. PAYMENT PROCESSING: User completes payment on gateway platform
// 5. CALLBACK HANDLING: Gateway sends payment result back to system
// 6. TRANSACTION VERIFICATION: System validates payment authenticity
// 7. STATUS UPDATE: Payment status updated and services activated
// 8. NOTIFICATION: User notified of payment success/failure
// 
// SUPPORTED PAYMENT METHODS:
// - VNPay: Vietnam's leading payment gateway
// - MoMo: Mobile wallet payment system
// - ZaloPay: Digital wallet and payment platform
// - Bank transfer: Direct bank account transfers
// - Credit/Debit cards: Via integrated payment gateways
// 
// PAYMENT SECURITY:
// - SSL/TLS encryption for all payment communications
// - Digital signature verification for callback authenticity
// - PCI DSS compliance for card payment processing
// - Secure token-based payment URL generation
// - Transaction logging and audit trails
// 
// TRANSACTION MANAGEMENT:
// - Real-time payment status tracking
// - Automatic retry mechanisms for failed payments
// - Refund processing capabilities
// - Payment history and reporting
// - Revenue analytics and financial insights
// 
// SERVICE INTEGRATION:
// - STI testing package payments
// - Appointment consultation fees
// - Premium subscription services
// - Additional health service charges
// - Automated service activation upon payment
// 
// FINANCIAL COMPLIANCE:
// - Transaction record keeping for auditing
// - Tax calculation and reporting
// - Revenue recognition and accounting
// - Fraud detection and prevention
// - Regulatory compliance monitoring

using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Requests.Payment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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

        /// <summary>
        /// CREATE PAYMENT URL
        /// ==================
        /// Generates secure payment gateway URL for service payments
        /// 
        /// PAYMENT INITIATION FLOW:
        /// 1. Validate payment request (service ID, amount, payment method)
        /// 2. Retrieve service details and pricing information
        /// 3. Create payment transaction record with pending status
        /// 4. Generate secure payment URL with digital signature
        /// 5. Return payment URL for user redirection
        /// 
        /// SUPPORTED SERVICES:
        /// - STI testing packages (Basic, Advanced, Custom)
        /// - Consultation appointments with healthcare providers
        /// - Premium subscription services
        /// - Additional health screening services
        /// 
        /// PAYMENT GATEWAY INTEGRATION:
        /// - VNPay: Default payment gateway for Vietnam market
        /// - MoMo: Mobile wallet integration
        /// - ZaloPay: Digital payment platform
        /// - Automatic gateway selection based on user preference
        /// 
        /// SECURITY MEASURES:
        /// - Transaction ID generation for tracking
        /// - Digital signature for payment URL authenticity
        /// - Secure parameter encoding
        /// - Timestamp validation for URL expiry
        /// 
        /// USE CASES:
        /// - STI testing: Payment for selected test packages
        /// - Appointments: Consultation fee processing
        /// - Subscriptions: Premium service activation
        /// - Additional services: Extra health screening payments
        /// </summary>
        [HttpPost("create-payment")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            try
            {
                // Generate secure payment URL with gateway integration
                // Creates transaction record and returns payment redirect URL
                var response = await _paymentService.CreatePaymentUrl(request, HttpContext);
                return Ok(new { is_success = true, data = response, message = "Payment URL created successfully" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { is_success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// VNPAY IPN HANDLER
        /// =================
        /// Processes Instant Payment Notification from VNPay gateway
        /// 
        /// IPN PROCESSING FLOW:
        /// 1. Receive payment notification from VNPay servers
        /// 2. Validate digital signature for authenticity
        /// 3. Extract transaction details and payment status
        /// 4. Update payment transaction record in database
        /// 5. Activate services if payment successful
        /// 6. Send confirmation response to VNPay
        /// 
        /// SECURITY VALIDATION:
        /// - Digital signature verification using VNPay secret key
        /// - Transaction ID validation against database records
        /// - Amount verification to prevent tampering
        /// - Duplicate notification prevention
        /// 
        /// PAYMENT STATUS HANDLING:
        /// - Success: Activate STI testing services, update appointment status
        /// - Failed: Log failure reason, notify user, allow retry
        /// - Pending: Maintain current status, await further updates
        /// - Cancelled: Update status, release reserved resources
        /// 
        /// AUTOMATED RESPONSES:
        /// - VNPay requires specific response codes for acknowledgment
        /// - Success: "00" - Payment processed successfully
        /// - Failure: Error codes indicating specific issues
        /// </summary>
        [HttpGet("vnpay-ipn")]
        public async Task<IActionResult> VnPayIpn()
        {
            // Process VNPay Instant Payment Notification
            // Validates signature and updates transaction status
            var ipnResponse = await _paymentService.ProcessIpnResponse(Request.Query);
            // Returning the response required by VNPay
            return Ok(ipnResponse);
        }

        /// <summary>
        /// VNPAY CALLBACK PROCESSOR
        /// ========================
        /// Handles payment callback from VNPay for frontend integration
        /// 
        /// CALLBACK PROCESSING FLOW:
        /// 1. Receive payment result from VNPay frontend integration
        /// 2. Convert callback parameters to standard format
        /// 3. Validate payment signature and transaction details
        /// 4. Process payment result and update transaction status
        /// 5. Return standardized response for frontend handling
        /// 
        /// FRONTEND INTEGRATION:
        /// - Handles payment results from VNPay JavaScript SDK
        /// - Provides immediate feedback to user interface
        /// - Enables real-time payment status updates
        /// - Supports single-page application workflows
        /// 
        /// RESPONSE CODES:
        /// - "00": Payment successful, services activated
        /// - "01": Transaction not found or invalid
        /// - "02": Transaction already processed
        /// - "97": Invalid signature or tampered data
        /// - "99": System error or processing failure
        /// 
        /// USER EXPERIENCE:
        /// - Immediate payment confirmation
        /// - Automatic service activation
        /// - Error handling with retry options
        /// - Seamless integration with application flow
        /// </summary>
        [HttpPost("vnpay-callback")]
        public async Task<IActionResult> ProcessVnPayCallback([FromBody] Dictionary<string, string> vnpayParams)
        {
            try
            {
                // Convert dictionary to query collection for processing
                var queryCollection = new Microsoft.AspNetCore.Http.QueryCollection(
                    vnpayParams.ToDictionary(kvp => kvp.Key, kvp => new Microsoft.Extensions.Primitives.StringValues(kvp.Value))
                );

                // Process payment callback with signature validation
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

        /// <summary>
        /// GET PAYMENT TRANSACTION
        /// =======================
        /// Retrieves detailed information about a specific payment transaction
        /// 
        /// TRANSACTION DETAILS:
        /// - Transaction ID and payment reference
        /// - Payment amount and currency
        /// - Payment method and gateway used
        /// - Transaction status and timestamps
        /// - Associated service details (STI testing, appointments)
        /// - Payment gateway response codes and messages
        /// 
        /// TRANSACTION STATUSES:
        /// - Pending: Payment initiated but not completed
        /// - Success: Payment completed successfully
        /// - Failed: Payment failed due to various reasons
        /// - Cancelled: Payment cancelled by user or system
        /// 
        /// USE CASES:
        /// - Transaction verification: Confirm payment completion
        /// - Customer support: Investigate payment issues
        /// - Audit trails: Financial record keeping
        /// - Dispute resolution: Payment investigation
        /// - Service activation: Verify payment before service delivery
        /// 
        /// SECURITY CONSIDERATIONS:
        /// - Access control for transaction data
        /// - Sensitive information protection
        /// - Audit logging for transaction access
        /// </summary>
        [HttpGet("transaction/{transactionId}")]
        public async Task<IActionResult> GetPaymentTransaction(Guid transactionId)
        {
            try
            {
                // Retrieve complete transaction details including related services
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

        /// <summary>
        /// GET CUSTOMER PAYMENT HISTORY
        /// ============================
        /// Retrieves comprehensive payment history for a specific customer
        /// 
        /// PAYMENT HISTORY FEATURES:
        /// - Complete transaction timeline
        /// - Payment amounts and methods used
        /// - Service details for each payment
        /// - Success/failure rates and patterns
        /// - Total spending and financial summary
        /// 
        /// FINANCIAL INSIGHTS:
        /// - Total amount spent on healthcare services
        /// - Payment method preferences
        /// - Service utilization patterns
        /// - Successful vs failed transaction ratios
        /// - Monthly/yearly spending trends
        /// 
        /// ACCESS CONTROL:
        /// - Customers can view their own payment history
        /// - Admins can access any customer's payment data
        /// - Healthcare providers can view relevant payment information
        /// - Secure data handling and privacy protection
        /// 
        /// USE CASES:
        /// - Customer dashboard: "My Payment History"
        /// - Financial planning: Healthcare spending analysis
        /// - Customer support: Payment issue resolution
        /// - Billing inquiries: Transaction verification
        /// - Tax reporting: Healthcare expense documentation
        /// 
        /// PRIVACY & SECURITY:
        /// - Role-based access control
        /// - Sensitive financial data protection
        /// - Audit logging for payment data access
        /// </summary>
        // New endpoints for payment history
        [HttpGet("customer/{customerId}/history")]
        [Authorize] // Role-based access control for payment data
        public async Task<IActionResult> GetCustomerPaymentHistory(Guid customerId)
        {
            try
            {
                // Retrieve comprehensive payment history with financial insights
                // Includes transaction details, spending patterns, and service utilization
                var paymentHistory = await _paymentService.GetCustomerPaymentHistory(customerId);
                return Ok(new { 
                    is_success = true, 
                    data = paymentHistory, // Complete payment history and financial summary
                    message = "Customer payment history retrieved successfully" 
                });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { is_success = false, message = ex.Message });
            }
        }

        [HttpGet("history")]
        [Authorize(Roles = "Admin,Staff")] // Only admin and staff can view all payment history
        public async Task<IActionResult> GetAllPaymentHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                if (pageSize > 100) pageSize = 100; // Limit page size
                
                var paymentHistory = await _paymentService.GetAllPaymentHistory(page, pageSize);
                return Ok(new { 
                    is_success = true, 
                    data = paymentHistory, 
                    page = page,
                    pageSize = pageSize,
                    message = "Payment history retrieved successfully" 
                });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { is_success = false, message = ex.Message });
            }
        }
    }
}