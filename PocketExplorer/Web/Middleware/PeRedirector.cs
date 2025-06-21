using PocketExplorer.Web;
using PocketExplorer.Web.Utils;

namespace PocketExplorer
{
    public class PeRedirector(RequestDelegate next, DataKeeper dataKeeper)
    {
        private readonly RequestDelegate _next = next;
        private readonly DataKeeper _dataKeeper = dataKeeper;

        public async Task Invoke(HttpContext httpContext)
        {
            var locations = _dataKeeper.PeInstance.Locations;

            var reqPath = httpContext.Request.Path.Value?.Trim('/') ?? "";

            var authTag = (int) httpContext.Items["Auth-Tag"]!;
            if (authTag == 1)
            {
                httpContext.Request.Path = "/auth";
                await _next(httpContext);
                return;
            }

            if (httpContext.Request.Method == "GET" && !string.IsNullOrWhiteSpace(reqPath))
            {
                var splitIndex = reqPath.IndexOf('/');

                string locationName;
                string targetPath;
                if (splitIndex == -1)
                {
                    locationName = reqPath;
                    targetPath = "";
                }
                else
                {
                    locationName = reqPath[..splitIndex];
                    targetPath = reqPath[splitIndex..];
                }

                var locationRoot = locations.SingleOrDefault(lc => lc.Name == locationName)?.Path;
                if (locationRoot is null)
                {
                    httpContext.Response.StatusCode = 404;
                    return;
                }

                var fullPath = FileHelper.GetFullPath(locationRoot, targetPath);
                if (fullPath is null)
                {
                    httpContext.Response.StatusCode = 404;
                    return;
                }

                if (File.Exists(fullPath))
                {
                    httpContext.Request.Path = $"/file/{reqPath}";
                }
                else
                {
                    httpContext.Request.Path = $"/browse/{reqPath}";
                }
            }

            await _next(httpContext);
        }

    }

    public static class FileRedirectorExtensions
    {
        public static IApplicationBuilder UsePeRedirector(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PeRedirector>();
        }
    }
}
