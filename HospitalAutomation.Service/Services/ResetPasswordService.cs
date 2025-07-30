using HospitalAutomation.DataAccess.Context;
using HospitalAutomation.DataAccess.DTOs;
using HospitalAutomation.Service.Interfaces;
using HospitalAutomation.Service.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HospitalAutomation.DataAccess.Models;  

using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace HospitalAutomation.Service.Services
{
    public class ResetPasswordService : IResetPasswordService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ResetPasswordService> _logger;
        private readonly IUserService _userService;

        public ResetPasswordService(AppDbContext context, ILogger<ResetPasswordService> logger, IUserService userService)
        {
            _context = context;
            _logger = logger;
            _userService = userService;

        }

        private string HashPassword(string password)
        {
            string secretKey = "*ZCkiEre7EbhJ_k";
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var combinedPassword = password + secretKey;
            var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(combinedPassword));
            return Convert.ToBase64String(bytes);
        }
        private string GenerateCode(int length = 6)
        {
            const string chars = "0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        public async Task<ResponseGeneric<bool>> ResetPasswordAsync(ResetPasswordDto dto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (user == null)
                {
                    _logger.LogWarning("Şifre sıfırlama isteği: Email bulunamadı - {Email}", dto.Email);
                    return ResponseGeneric<bool>.Error("Kullanıcı bulunamadı.");
                }

                user.Password = HashPassword(dto.NewPassword);
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Şifre başarıyla sıfırlandı - Kullanıcı ID: {UserId}", user.Id);
                return ResponseGeneric<bool>.Success(true, "Şifre başarıyla sıfırlandı.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şifre sıfırlama sırasında hata oluştu.");
                return ResponseGeneric<bool>.Error("Şifre sıfırlama başarısız oldu.");
            }
        }
        private async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                // SMTP ayarlarını kendi mail sunucuna göre düzenle
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("fffaruk9@gmail.com", "pcxx gkny ykhb wftt"),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("fffaruk9@gmail.com", "Hastane Otomasyonu"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Mail gönderme hatası: " + ex.Message);

                if (ex.InnerException != null)
                    Console.WriteLine("Inner Exception: " + ex.InnerException.Message);

                return false;
            }

        }
        public async Task<ResponseGeneric<bool>> SendResetCodeAsync(ForgotPasswordRequestDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return ResponseGeneric<bool>.Error("Kayıtlı bir kullanıcı bulunamadı.");

            var code = GenerateCode();

            // Önce eski kodları temizleyebiliriz veya işaretleyebiliriz
            var existingCodes = _context.ResetPasswords.Where(c => c.Email == dto.Email && !c.Used);
            _context.ResetPasswords.RemoveRange(existingCodes);

            var resetCode = new ResetPassword
            {
                Email = dto.Email,
                Code = code,
                Expiration = DateTime.UtcNow.AddMinutes(15), // 15 dk geçerli
                Used = false
            };
            _context.ResetPasswords.Add(resetCode);
            await _context.SaveChangesAsync();

            var mailBody = $"<p>Şifre sıfırlama kodunuz: <strong>{code}</strong></p><p>15 dakika içerisinde kullanınız.</p>";
            var mailSent = await SendEmailAsync(dto.Email, "Şifre Sıfırlama Kodu", mailBody);

            if (!mailSent)
                return ResponseGeneric<bool>.Error("Mail gönderilemedi. Lütfen tekrar deneyiniz.");

            _logger.LogInformation("Şifre sıfırlama kodu gönderildi: {Email}", dto.Email);
            return ResponseGeneric<bool>.Success(true, "Şifre sıfırlama kodu mail olarak gönderildi.");
        }
        public async Task<ResponseGeneric<bool>> VerifyResetCodeAsync(VerifyResetCodeDto dto)
        {
            var codeRecord = await _context.ResetPasswords
                .Where(c => c.Email == dto.Email && c.Code == dto.Code && !c.Used && c.Expiration > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            if (codeRecord == null)
                return ResponseGeneric<bool>.Error("Geçersiz veya süresi dolmuş kod.");

            return ResponseGeneric<bool>.Success(true, "Kod doğrulandı.");
        }
    }
}
