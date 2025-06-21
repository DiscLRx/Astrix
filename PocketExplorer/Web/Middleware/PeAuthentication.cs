using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace PocketExplorer.Web.Middleware
{
    public class PeAuthentication
    {
        private readonly RequestDelegate _next;

        private readonly DataKeeper _dataKeeper;

        public PeAuthentication(RequestDelegate next, DataKeeper dataKeeper)
        {
            _dataKeeper = dataKeeper;
            _next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {
            if (_dataKeeper.PeInstance.IsLocked)
            {
                var password = GetPasswordFromCookies(httpContext);
                if (password != _dataKeeper.PeInstance.Password)
                {
                    httpContext.Items.Add("Auth-Tag", 1);
                    return _next(httpContext);
                }
            }
            httpContext.Items.Add("Auth-Tag", 0);
            return _next(httpContext);
        }

        public string? GetPasswordFromCookies(HttpContext httpContext)
        {
            if (httpContext.Request.Cookies.TryGetValue("Auth-Key", out var password))
            {
                return password;
            }
            return null;
        }
    }

    public static class AuthenticationExtensions
    {
        public static IApplicationBuilder UsePeAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PeAuthentication>();
        }
    }
}
