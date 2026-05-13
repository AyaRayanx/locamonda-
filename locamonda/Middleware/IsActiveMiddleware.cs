using locamonda.Models;
using Microsoft.AspNetCore.Identity;

namespace locamonda.Middleware
{
    public class IsActiveMiddleware
    {
        private readonly RequestDelegate _next;

        public IsActiveMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<Users> userManager, SignInManager<Users> signInManager)
        {
            if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
            {
                var user = await userManager.GetUserAsync(context.User);

                if (user == null || !user.IsActive)
                {
                    await signInManager.SignOutAsync();
                    context.Response.Redirect("/Users/Login?message=Your account has been deactivated.");
                    return;
                }
            }

            await _next(context);
        }
    }

    public static class IsActiveMiddlewareExtensions
    {
        public static IApplicationBuilder UseIsActiveCheck(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<IsActiveMiddleware>();
        }
    }
}
