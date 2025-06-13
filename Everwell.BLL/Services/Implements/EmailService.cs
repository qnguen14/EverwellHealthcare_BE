using Everwell.BLL.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Everwell.BLL.Services.Implements
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                Console.WriteLine($"Attempting to send email to: {toEmail}");
                Console.WriteLine($"SMTP Server: {_configuration["Email:SmtpServer"]}");
                Console.WriteLine($"SMTP Port: {_configuration["Email:SmtpPort"]}");
                Console.WriteLine($"Username: {_configuration["Email:Username"]}");
                Console.WriteLine($"From Email: {_configuration["Email:FromEmail"]}");

                var smtpClient = new SmtpClient(_configuration["Email:SmtpServer"])
                {
                    Port = int.Parse(_configuration["Email:SmtpPort"]),
                    Credentials = new NetworkCredential(
                        _configuration["Email:Username"], 
                        _configuration["Email:Password"]
                    ),
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 30000 // 30 seconds timeout
                };

                var message = new MailMessage(
                    from: _configuration["Email:FromEmail"],
                    to: toEmail,
                    subject: subject,
                    body: body
                )
                {
                    IsBodyHtml = true
                };

                Console.WriteLine("Sending email...");
                await smtpClient.SendMailAsync(message);
                Console.WriteLine($"Email sent successfully to {toEmail}");
            }
            catch (SmtpException smtpEx)
            {
                Console.WriteLine($"SMTP Error: {smtpEx.Message}");
                Console.WriteLine($"SMTP Status Code: {smtpEx.StatusCode}");
                throw new Exception($"SMTP Error: {smtpEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Email Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task SendPasswordResetCodeAsync(string toEmail, string resetCode, string userName)
        {
            var subject = "Password Reset Code - Everwell Health";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #4CAF50; color: white; padding: 20px; text-align: center;'>
                        <h2>Password Reset Code</h2>
                    </div>
                    <div style='padding: 30px; background-color: #f9f9f9;'>
                        <p>Hi {userName},</p>
                        <p>We received a request to reset your password. Use the verification code below:</p>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <div style='background-color: #ffffff; border: 2px solid #4CAF50; border-radius: 8px; padding: 20px; display: inline-block;'>
                                <h1 style='color: #4CAF50; margin: 0; font-size: 32px; letter-spacing: 8px;'>{resetCode}</h1>
                            </div>
                        </div>
                        
                        <p style='color: #666;'>This code will expire in <strong>15 minutes</strong>.</p>
                        <p style='color: #666;'>If you didn't request this, please ignore this email.</p>
                        
                        <div style='margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd;'>
                            <p style='color: #888; font-size: 12px;'>Best regards,<br>Everwell Health Team</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendMenstrualCycleReminderAsync(string toEmail, string userName, DateTime nextCycleDate)
        {
            var subject = "Menstrual Cycle Reminder - Everwell Health";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Cycle Reminder</h2>
                    <p>Hi {userName},</p>
                    <p>Your next cycle is expected on {nextCycleDate:MMMM dd, yyyy}.</p>
                    <p>Don't forget to track your symptoms in the Everwell app!</p>
                    <p>Best regards,<br>Everwell Health Team</p>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendAppointmentConfirmationAsync(string toEmail, string userName, DateTime appointmentDate, string serviceName)
        {
            var subject = "Appointment Confirmation - Everwell Health";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Appointment Confirmed</h2>
                    <p>Hi {userName},</p>
                    <p>Your appointment has been confirmed:</p>
                    <ul>
                        <li><strong>Service:</strong> {serviceName}</li>
                        <li><strong>Date:</strong> {appointmentDate:MMMM dd, yyyy}</li>
                        <li><strong>Time:</strong> {appointmentDate:HH:mm}</li>
                    </ul>
                    <p>Please arrive 15 minutes early.</p>
                    <p>Best regards,<br>Everwell Health Team</p>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, body);
        }
    }
}
