using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace PocketExplorer.Web.Middleware
{
    public class PeAuthentication(RequestDelegate next, DataKeeper dataKeeper)
    {
        private readonly RequestDelegate _next = next;

        private readonly DataKeeper _dataKeeper = dataKeeper;

        public async Task Invoke(HttpContext httpContext)
        {
            if (_dataKeeper.PeInstance.IsLocked)
            {
                var password = GetPasswordFromCookies(httpContext);
                if (password != _dataKeeper.PeInstance.Password)
                {
                    httpContext.Request.Path = "/auth";
                    await _next(httpContext);
                    return;
                }
            }
            await _next(httpContext);
        }

        private static string? GetPasswordFromCookies(HttpContext httpContext)
            => httpContext.Request.Cookies.TryGetValue("Auth-Key", out var password) ? password : null;
    }

    public static class AuthenticationExtensions
    {
        public static IApplicationBuilder UsePeAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PeAuthentication>();
        }
    }
}
