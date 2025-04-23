
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MFAApp.Middleware
{


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

}
