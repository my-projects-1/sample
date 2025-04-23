using System.Net.Mail;
using System.Net;

namespace MFAApp.Services
{
        public class EmailOtpService
        {
            private readonly IConfiguration _config;
            private readonly IHttpContextAccessor _httpContextAccessor;

            public EmailOtpService(IConfiguration config, IHttpContextAccessor httpContextAccessor)
            {
                _config = config;
                _httpContextAccessor = httpContextAccessor;
            }

            public async Task<string> GenerateAndSendOtpAsync(string userEmail)
            {
                var otp = new Random().Next(100000, 999999).ToString();
                _httpContextAccessor.HttpContext.Session.SetString("OtpCode", otp);

                var smtp = _config.GetSection("Smtp");
                using var client = new SmtpClient(smtp["Host"], int.Parse(smtp["Port"]))
                {
                    Credentials = new NetworkCredential(smtp["Username"], smtp["Password"]),
                    EnableSsl = true
                };

                var mail = new MailMessage(smtp["From"], userEmail)
                {
                    Subject = "Your MFA OTP Code",
                    Body = $"Your OTP code is {otp}"
                };

                await client.SendMailAsync(mail);
                return otp;
            }

            public bool VerifyOtp(string otp)
            {
                var storedOtp = _httpContextAccessor.HttpContext.Session.GetString("OtpCode");
                return otp == storedOtp;
            }
        }
   


}
