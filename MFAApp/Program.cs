using MFAApp.Services;
using MFAApp.Middleware;

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

