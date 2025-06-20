using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using AutoMapper;
using Everwell.BLL.Infrastructure;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Exceptions;
using Everwell.DAL.Data.Requests.Payment;
using Everwell.DAL.Data.Responses.Payment;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Query; // Required for IIncludableQueryable

namespace Everwell.BLL.Services.Implements
{
    public class PaymentService : BaseService<PaymentService>, IPaymentService
    {
        private readonly IConfiguration _configuration;

        public PaymentService(IUnitOfWork<EverwellDbContext> unitOfWork, ILogger<PaymentService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
            : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _configuration = configuration;
        }

        public async Task<CreatePaymentResponse> CreatePaymentUrl(CreatePaymentRequest request, HttpContext context)
        {
            // Validate if STI Testing exists and is not paid
            var stiTest = await _unitOfWork.GetRepository<STITesting>()
                .FirstOrDefaultAsync(
                    predicate: x => x.Id == request.StiTestingId,
                    orderBy: null,
                    include: null
                );

            if (stiTest == null)
            {
                throw new NotFoundException("STI Testing record not found.");
            }
            
            if (stiTest.IsPaid)
            {
                throw new BadRequestException("This STI Test has already been paid for.");
            }

            // Check if payment transaction already exists for this STI test
            var existingTransaction = await _unitOfWork.GetRepository<PaymentTransaction>()
                .FirstOrDefaultAsync(
                    predicate: t => t.StiTestingId == request.StiTestingId && t.Status == PaymentStatus.Pending,
                    orderBy: null,
                    include: null
                );

            PaymentTransaction transaction;
            
            if (existingTransaction != null)
            {
                // Use existing pending transaction
                transaction = existingTransaction;
            }
            else
            {
                // Create new payment transaction
                transaction = new PaymentTransaction
                {
                    StiTestingId = stiTest.Id,
                    Amount = stiTest.TotalPrice,
                    Status = PaymentStatus.Pending,
                    PaymentMethod = request.PaymentMethod ?? "VNPay",
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.GetRepository<PaymentTransaction>().InsertAsync(transaction);
                await _unitOfWork.SaveChangesAsync();
            }

            // Handle different payment methods
            switch (request.PaymentMethod?.ToLower())
            {
                case "vnpay":
                case null: // Default to VNPay
                    return await CreateVnPayUrl(transaction, stiTest, context);
                
                case "momo":
                    return await CreateMoMoUrl(transaction, stiTest);
                
                case "zalopay":
                    return await CreateZaloPayUrl(transaction, stiTest);
                
                default:
                    throw new BadRequestException($"Payment method '{request.PaymentMethod}' is not supported.");
            }
        }

        private async Task<CreatePaymentResponse> CreateVnPayUrl(PaymentTransaction transaction, STITesting stiTest, HttpContext context)
        {
            var vnpay = new VnPayLibrary();
            var vnPayConfig = _configuration.GetSection("VnPay");

            // Validate VNPay configuration
            if (string.IsNullOrEmpty(vnPayConfig["TmnCode"]) || string.IsNullOrEmpty(vnPayConfig["HashSecret"]))
            {
                throw new Exception("VNPay configuration is missing or invalid.");
            }

            vnpay.AddRequestData("vnp_Version", vnPayConfig["Version"] ?? "2.1.0");
            vnpay.AddRequestData("vnp_Command", vnPayConfig["Command"] ?? "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnPayConfig["TmnCode"]);
            vnpay.AddRequestData("vnp_Amount", ((long)stiTest.TotalPrice * 100).ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", GetClientIpAddress(context));
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan xet nghiem STI #{stiTest.Id.ToString("N")[..8]}");
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", vnPayConfig["ReturnUrl"] ?? "https://localhost:3000/vnpay-callback");
            vnpay.AddRequestData("vnp_TxnRef", transaction.Id.ToString());

            string paymentUrl = vnpay.CreateRequestUrl(
                vnPayConfig["BaseUrl"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html", 
                vnPayConfig["HashSecret"]
            );

            // Update transaction with VNPay URL
            transaction.OrderInfo = $"VNPay payment for STI Test {stiTest.Id}";
            _unitOfWork.GetRepository<PaymentTransaction>().UpdateAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            return new CreatePaymentResponse 
            { 
                PaymentUrl = paymentUrl, 
                PaymentId = transaction.Id,
                PaymentMethod = "VNPay"
            };
        }

        private async Task<CreatePaymentResponse> CreateMoMoUrl(PaymentTransaction transaction, STITesting stiTest)
        {
            // For demo purposes - in real implementation, integrate with MoMo API
            await Task.Delay(1); // Simulate async operation
            
            var demoUrl = $"https://demo-momo.com/payment?amount={stiTest.TotalPrice}&orderId={transaction.Id}";
            
            transaction.OrderInfo = $"MoMo payment for STI Test {stiTest.Id}";
            _unitOfWork.GetRepository<PaymentTransaction>().UpdateAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            return new CreatePaymentResponse 
            { 
                PaymentUrl = demoUrl, 
                PaymentId = transaction.Id,
                PaymentMethod = "MoMo"
            };
        }

        private async Task<CreatePaymentResponse> CreateZaloPayUrl(PaymentTransaction transaction, STITesting stiTest)
        {
            // For demo purposes - in real implementation, integrate with ZaloPay API
            await Task.Delay(1); // Simulate async operation
            
            var demoUrl = $"https://demo-zalopay.com/payment?amount={stiTest.TotalPrice}&orderId={transaction.Id}";
            
            transaction.OrderInfo = $"ZaloPay payment for STI Test {stiTest.Id}";
            _unitOfWork.GetRepository<PaymentTransaction>().UpdateAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            return new CreatePaymentResponse 
            { 
                PaymentUrl = demoUrl, 
                PaymentId = transaction.Id,
                PaymentMethod = "ZaloPay"
            };
        }

        private string GetClientIpAddress(HttpContext context)
        {
            var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            }
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = context.Connection.RemoteIpAddress?.ToString();
            }
            return ipAddress ?? "127.0.0.1";
        }

        public async Task<PaymentIpnResponse> ProcessIpnResponse(IQueryCollection vnpayData)
        {
            var vnpay = new VnPayLibrary();
            
            // Add all VNPay response data
            foreach (var (key, value) in vnpayData)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value.ToString());
                }
            }

            var vnp_TxnRef = vnpay.GetResponseData("vnp_TxnRef");
            var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
            var vnp_SecureHash = vnpayData["vnp_SecureHash"].FirstOrDefault();

            // Validate transaction reference
            if (!Guid.TryParse(vnp_TxnRef, out var transactionId))
            {
                _logger.LogWarning("VNPay IPN: Invalid transaction reference {TxnRef}", vnp_TxnRef);
                return new PaymentIpnResponse { RspCode = "99", Message = "Invalid transaction reference." };
            }

            // Validate signature
            var vnPayConfig = _configuration.GetSection("VnPay");
            bool isValidSignature = vnpay.ValidateSignature(vnp_SecureHash, vnPayConfig["HashSecret"]);

            if (!isValidSignature)
            {
                _logger.LogWarning("VNPay IPN: Invalid signature for transaction {TxnRef}", vnp_TxnRef);
                return new PaymentIpnResponse { RspCode = "97", Message = "Invalid signature." };
            }

            // Get transaction with STI testing
            var transaction = await _unitOfWork.GetRepository<PaymentTransaction>()
                .FirstOrDefaultAsync(
                    predicate: t => t.Id == transactionId,
                    orderBy: null,
                    include: source => source.Include(p => p.StiTesting)
                );

            if (transaction == null)
            {
                _logger.LogWarning("VNPay IPN: Transaction not found {TxnRef}", vnp_TxnRef);
                return new PaymentIpnResponse { RspCode = "01", Message = "Order not found." };
            }

            if (transaction.StiTesting == null)
            {
                _logger.LogError("VNPay IPN: STI Testing not found for transaction {TxnRef}", vnp_TxnRef);
                return new PaymentIpnResponse { RspCode = "99", Message = "Related STI Testing not found." };
            }

            // Check if already processed
            if (transaction.Status != PaymentStatus.Pending)
            {
                _logger.LogInformation("VNPay IPN: Transaction already processed {TxnRef} with status {Status}", vnp_TxnRef, transaction.Status);
                return new PaymentIpnResponse { RspCode = "02", Message = "Order already confirmed." };
            }

            // Update transaction status
            bool isSuccess = (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00");
            transaction.Status = isSuccess ? PaymentStatus.Success : PaymentStatus.Failed;
            transaction.ResponseCode = vnp_ResponseCode;
            transaction.TransactionId = vnpay.GetResponseData("vnp_TransactionNo");
            transaction.UpdatedAt = DateTime.UtcNow;

            // Update STI Testing payment status if successful
            if (isSuccess)
            {
                transaction.StiTesting.IsPaid = true;
                _unitOfWork.GetRepository<STITesting>().UpdateAsync(transaction.StiTesting);
                _logger.LogInformation("VNPay IPN: Payment successful for STI Testing {StiTestId}", transaction.StiTesting.Id);
            }
            else
            {
                _logger.LogWarning("VNPay IPN: Payment failed for transaction {TxnRef} with code {ResponseCode}", vnp_TxnRef, vnp_ResponseCode);
            }

            _unitOfWork.GetRepository<PaymentTransaction>().UpdateAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            return new PaymentIpnResponse { RspCode = "00", Message = "Confirm success." };
        }

        public async Task<PaymentTransaction> GetPaymentTransaction(Guid transactionId)
        {
            return await _unitOfWork.GetRepository<PaymentTransaction>()
                .FirstOrDefaultAsync(
                    predicate: t => t.Id == transactionId,
                    orderBy: null,
                    include: source => source.Include(p => p.StiTesting)
                );
        }

        public async Task<CustomerPaymentHistoryResponse> GetCustomerPaymentHistory(Guid customerId)
        {
            // Get customer info
            var customer = await _unitOfWork.GetRepository<User>()
                .FirstOrDefaultAsync(predicate: u => u.Id == customerId);

            if (customer == null)
            {
                throw new NotFoundException("Customer not found.");
            }

            // Get all payment transactions for this customer through STI Testing
            var transactions = await _unitOfWork.GetRepository<PaymentTransaction>()
                .GetListAsync(
                    predicate: t => t.StiTesting.CustomerId == customerId,
                    orderBy: query => query.OrderByDescending(t => t.CreatedAt),
                    include: source => source.Include(p => p.StiTesting)
                );

            var paymentHistory = transactions.Select(t => new PaymentHistoryResponse
            {
                TransactionId = t.Id,
                StiTestingId = t.StiTestingId,
                Amount = t.Amount,
                Status = t.Status,
                PaymentMethod = t.PaymentMethod,
                TransactionReference = t.TransactionId,
                OrderInfo = t.OrderInfo,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                TestPackage = t.StiTesting?.TestPackage.ToString() ?? "Unknown",
                ScheduleDate = t.StiTesting?.ScheduleDate?.ToDateTime(TimeOnly.MinValue),
                TestingStatus = t.StiTesting?.Status?.ToString() ?? "Unknown"
            }).ToList();

            var totalAmount = transactions.Sum(t => t.Amount);
            var totalSuccessfulAmount = transactions
                .Where(t => t.Status == PaymentStatus.Success)
                .Sum(t => t.Amount);

            return new CustomerPaymentHistoryResponse
            {
                CustomerId = customerId,
                CustomerName = customer.Name,
                CustomerEmail = customer.Email,
                TotalTransactions = transactions.Count,
                TotalAmount = totalAmount,
                TotalSuccessfulAmount = totalSuccessfulAmount,
                PaymentHistory = paymentHistory
            };
        }

        public async Task<List<PaymentHistoryResponse>> GetAllPaymentHistory(int page = 1, int pageSize = 20)
        {
            // Get all transactions first, then apply pagination manually
            var allTransactions = await _unitOfWork.GetRepository<PaymentTransaction>()
                .GetListAsync(
                    predicate: null,
                    orderBy: query => query.OrderByDescending(t => t.CreatedAt),
                    include: source => source.Include(p => p.StiTesting).ThenInclude(s => s.Customer)
                );

            // Apply pagination manually
            var skip = (page - 1) * pageSize;
            var transactions = allTransactions.Skip(skip).Take(pageSize).ToList();

            return transactions.Select(t => new PaymentHistoryResponse
            {
                TransactionId = t.Id,
                StiTestingId = t.StiTestingId,
                Amount = t.Amount,
                Status = t.Status,
                PaymentMethod = t.PaymentMethod,
                TransactionReference = t.TransactionId,
                OrderInfo = t.OrderInfo,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                TestPackage = t.StiTesting?.TestPackage.ToString() ?? "Unknown",
                ScheduleDate = t.StiTesting?.ScheduleDate?.ToDateTime(TimeOnly.MinValue),
                TestingStatus = t.StiTesting?.Status?.ToString() ?? "Unknown"
            }).ToList();
        }
    }
}