using MFAApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace MFAApp.Controllers
{
        public class MfaController : Controller
        {
            private readonly EmailOtpService _emailOtp;
            private readonly TotpService _totp;

            public MfaController(EmailOtpService emailOtp, TotpService totp)
            {
                _emailOtp = emailOtp;
                _totp = totp;
            }

            // MFA method selection screen
            [HttpGet]
            public IActionResult Select()
            {
                return View();
            }

            [HttpPost]
            public IActionResult Select(string method)
            {
                return method switch
                {
                    "email" => RedirectToAction("ChallengeEmail"),
                    "totp" => RedirectToAction("ChallengeTotp"),
                    _ => RedirectToAction("Select")
                };
            }

            // Challenge via Email OTP
            [HttpGet]
            public async Task<IActionResult> ChallengeEmail()
            {
                var email = $"{User.Identity.Name}@yourdomain.com"; // Customize this!
                await _emailOtp.GenerateAndSendOtpAsync(email);
                ViewBag.Email = email;
                return View();
            }

            [HttpPost]
            public IActionResult ChallengeEmail(string otp)
            {
                if (_emailOtp.VerifyOtp(otp))
                {
                    HttpContext.Session.SetString("MfaCompleted", "true");
                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Error = "Invalid OTP. Please try again.";
                return View();
            }

            // Challenge via Authenticator App (TOTP)
            [HttpGet]
            public IActionResult ChallengeTotp()
            {
                var username = User.Identity.Name;
                var uri = _totp.GenerateSetupCode(username);
                ViewBag.QrCodeUrl = $"https://api.qrserver.com/v1/create-qr-code/?data={Uri.EscapeDataString(uri)}&size=200x200";
                return View();
            }

            [HttpPost]
            public IActionResult ChallengeTotp(string code)
            {
                var username = User.Identity.Name;
                if (_totp.VerifyCode(username, code))
                {
                    HttpContext.Session.SetString("MfaCompleted", "true");
                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Error = "Invalid authenticator code.";
                return View();
            }
        }
    


}
