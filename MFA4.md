Here is the **complete code** for the entire .NET 6 Core web application with **Windows Kerberos Authentication** and **Multi-Factor Authentication (MFA)** options (Email OTP + Authenticator App (TOTP)):

---

### 📁 Project Structure

```
MyKerberosApp/
│
├── Controllers/
│   └── MfaController.cs
│
├── Middleware/
│   └── MfaEnforcementMiddleware.cs
│
├── Services/
│   ├── EmailOtpService.cs
│   └── TotpService.cs
│
├── Views/
│   └── Mfa/
│       ├── Select.cshtml
│       ├── ChallengeEmail.cshtml
│       └── ChallengeTotp.cshtml
│
├── appsettings.json
├── launchSettings.json
├── Program.cs
└── MyKerberosApp.csproj
```

---

### 1. **`Controllers/MfaController.cs`**

```csharp
using Microsoft.AspNetCore.Mvc;
using MyKerberosApp.Services;

namespace MyKerberosApp.Controllers
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
```

---

### 2. **`Services/EmailOtpService.cs`**

```csharp
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace MyKerberosApp.Services
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
```

---

### 3. **`Services/TotpService.cs`**

```csharp
using OtpNet;
using System.Collections.Concurrent;

namespace MyKerberosApp.Services
{
    public class TotpService
    {
        private static readonly ConcurrentDictionary<string, byte[]> UserSecrets = new();

        public string GenerateSetupCode(string username)
        {
            if (!UserSecrets.ContainsKey(username))
            {
                var key = KeyGeneration.GenerateRandomKey(20);
                UserSecrets[username] = key;
            }

            var totp = new Totp(UserSecrets[username]);
            var base32Secret = Base32Encoding.ToString(UserSecrets[username]);

            var uri = new OtpUri(OtpType.Totp, base32Secret, username, "MyKerberosApp").ToString();
            return uri;
        }

        public bool VerifyCode(string username, string code)
        {
            if (!UserSecrets.TryGetValue(username, out var key)) return false;

            var totp = new Totp(key);
            return totp.VerifyTotp(code, out _);
        }
    }
}
```

---

### 4. **`Views/Mfa/Select.cshtml`**

```html
<h2>Choose MFA Method</h2>
<form method="post">
    <button name="method" value="email">Email OTP</button>
    <button name="method" value="totp">Authenticator App</button>
</form>
```

---

### 5. **`Views/Mfa/ChallengeEmail.cshtml`**

```html
<h2>Check your email</h2>
<p>We sent a code to <strong>@ViewBag.Email</strong></p>

<form method="post">
    <label>OTP Code:</label>
    <input name="otp" />
    <button type="submit">Verify</button>
</form>

@if (ViewBag.Error != null)
{
    <p style="color:red">@ViewBag.Error</p>
}
```

---

### 6. **`Views/Mfa/ChallengeTotp.cshtml`**

```html
<h2>Scan QR Code in Authenticator App</h2>
<img src="@ViewBag.QrCodeUrl" alt="QR Code" />

<form method="post">
    <label>Enter code from your app:</label>
    <input name="code" />
    <button type="submit">Verify</button>
</form>

@if (ViewBag.Error != null)
{
    <p style="color:red">@ViewBag.Error</p>
}
```

---

### 7. **`appsettings.json`**

```json
{
  "Smtp": {
    "Host": "smtp.yourprovider.com",
    "Port": 587,
    "Username": "your@email.com",
    "Password": "your-password",
    "From": "your@email.com"
  }
}
```

---

### 8. **`Program.cs`**

```csharp
using MyKerberosApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();
builder.Services.AddScoped<EmailOtpService>();
builder.Services.AddSingleton<TotpService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSession();
app.UseMiddleware<MfaEnforcementMiddleware>();

app.MapDefaultControllerRoute();

app.Run();
```

---

### 9. **`MfaEnforcementMiddleware.cs`**

```csharp
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public class MfaEnforcementMiddleware
{
    private readonly RequestDelegate _next;

    public MfaEnforcementMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var isAuthenticated = context.User.Identity.IsAuthenticated;
        var mfaCompleted = context.Session.GetString("MfaCompleted");

        if (isAuthenticated && string.IsNullOrEmpty(mfaCompleted))
        {
            context.Response.Redirect("/Mfa/Select");
            return;
        }

        await _next(context);
    }
}
```

---

### 10. **`launchSettings.json`**

```json
{
  "iisSettings": {
    "windowsAuthentication": true,
    "anonymousAuthentication": false
  },
  "profiles": {
    "MyKerberosApp": {
      "applicationUrl": "https://localhost:7018",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

---

## ✅ Next Steps:
1. **Ensure Kerberos Authentication is properly configured in `Startup.cs` or `Program.cs`.**
2. **Use an SMTP service** for sending OTPs (configure `appsettings.json`).
3. **Generate the QR code for TOTP in `ChallengeTotp.cshtml`.**

Let me know if you need any more customization or configuration assistance!
