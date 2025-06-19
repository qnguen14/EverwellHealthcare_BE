using AutoMapper;
using Everwell.BLL.Infrastructure;
using Everwell.BLL.Services.Interfaces;
using Everwell.DAL.Data.Entities;
using Everwell.DAL.Data.Exceptions;
using Everwell.DAL.Data.Requests.Payment;
using Everwell.DAL.Data.Responses.Payment;
using Everwell.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
            var stiTest = await _unitOfWork.GetRepository<STITesting>()
                .FirstOrDefaultAsync(
                    predicate: x => x.Id == request.StiTestingId,
                    orderBy: null, // Explicitly null
                    include: null  // Explicitly null
                );

            if (stiTest == null)
            {
                throw new NotFoundException("STI Testing record not found.");
            }
            
            if (stiTest.IsPaid)
            {
                throw new BadRequestException("This STI Test has already been paid for.");
            }

            var transaction = new PaymentTransaction
            {
                StiTestingId = stiTest.Id,
                Amount = stiTest.TotalPrice,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.GetRepository<PaymentTransaction>().InsertAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            var vnpay = new VnPayLibrary();
            var vnPayConfig = _configuration.GetSection("VnPay");

            vnpay.AddRequestData("vnp_Version", vnPayConfig["Version"]);
            vnpay.AddRequestData("vnp_Command", vnPayConfig["Command"]);
            vnpay.AddRequestData("vnp_TmnCode", vnPayConfig["TmnCode"]);
            vnpay.AddRequestData("vnp_Amount", ((long)stiTest.TotalPrice * 100).ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", $"Payment for STI Test {stiTest.Id}");
            vnpay.AddRequestData("vnp_OrderType", "other"); 
            vnpay.AddRequestData("vnp_ReturnUrl", vnPayConfig["ReturnUrl"]);
            vnpay.AddRequestData("vnp_TxnRef", transaction.Id.ToString());

            string paymentUrl = vnpay.CreateRequestUrl(vnPayConfig["BaseUrl"], vnPayConfig["HashSecret"]);

            return new CreatePaymentResponse { PaymentUrl = paymentUrl, PaymentId = transaction.Id };
        }

        public async Task<PaymentIpnResponse> ProcessIpnResponse(IQueryCollection vnpayData)
        {
            var vnpay = new VnPayLibrary();
            foreach (var (key, value) in vnpayData)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value.ToString());
                }
            }

            var vnp_TxnRef = vnpay.GetResponseData("vnp_TxnRef");
            if (!Guid.TryParse(vnp_TxnRef, out var transactionId))
            {
                return new PaymentIpnResponse { RspCode = "99", Message = "Invalid transaction reference." };
            }

            var vnp_SecureHash = vnpayData["vnp_SecureHash"].FirstOrDefault();
            var vnPayConfig = _configuration.GetSection("VnPay");
            bool isValidSignature = vnpay.ValidateSignature(vnp_SecureHash, vnPayConfig["HashSecret"]);

            if (!isValidSignature)
            {
                _logger.LogWarning($"VNPay IPN Invalid Signature. Received: {vnp_SecureHash}");
                return new PaymentIpnResponse { RspCode = "97", Message = "Invalid signature." };
            }
            
            var transaction = await _unitOfWork.GetRepository<PaymentTransaction>()
                .FirstOrDefaultAsync(
                    predicate: t => t.Id == transactionId,
                    orderBy: null, // Explicitly null
                    include: source => source.Include(p => p.StiTesting)
                );

            if (transaction == null)
            {
                return new PaymentIpnResponse { RspCode = "01", Message = "Order not found." };
            }

            if (transaction.StiTesting == null)
            {
                _logger.LogError($"Critical error: STI Testing entity not loaded for transaction {transaction.Id}");
                return new PaymentIpnResponse { RspCode = "99", Message = "Internal server error: related data missing." };
            }
            
            if (transaction.Status != PaymentStatus.Pending)
            {
                 return new PaymentIpnResponse { RspCode = "02", Message = "Order already confirmed." };
            }

            var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");

            transaction.Status = (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                ? PaymentStatus.Success
                : PaymentStatus.Failed;
                
            transaction.ResponseCode = vnp_ResponseCode;
            transaction.TransactionId = vnpay.GetResponseData("vnp_TransactionNo"); 
            transaction.OrderInfo = vnpay.GetResponseData("vnp_OrderInfo");
            transaction.UpdatedAt = DateTime.UtcNow;

            if (transaction.Status == PaymentStatus.Success)
            {
                transaction.StiTesting.IsPaid = true;
                _unitOfWork.GetRepository<STITesting>().UpdateAsync(transaction.StiTesting);
            }
            
            _unitOfWork.GetRepository<PaymentTransaction>().UpdateAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            return new PaymentIpnResponse { RspCode = "00", Message = "Confirm success." };
        }
    }
}