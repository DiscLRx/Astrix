using PocketExplorer.Data;
using PocketExplorer.Web;
using PocketExplorer.Web.Controllers;
using PocketExplorer.Web.Pages;
using System.Net;

namespace PocketExplorer;

public class PeHost
{
    private readonly WebApplication _app;

    public PeHost(PeInstance instance)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddConfiguration();
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Limits.MaxRequestBodySize = null;
            options.Listen(IPAddress.Parse("0.0.0.0"), instance.Port);
        });
        builder.Services.AddControllers()
            .AddApplicationPart(typeof(FileBrowseModel).Assembly)
        .AddApplicationPart(typeof(FileUploadController).Assembly)
        .AddApplicationPart(typeof(FileRangeController).Assembly);
        builder.Services.AddRazorPages(configure =>
        {
            configure.RootDirectory = "/Web/Pages";
        });
        builder.Services.AddSingleton<DataKeeper>();
        _app = builder.Build();
        _app.Services.GetRequiredService<DataKeeper>().PeInstance = instance;
        _app.UseFileRedirector();
        _app.UseRouting();
        _app.MapControllers();
        _app.MapRazorPages();
    }

    public async Task StartAsync()
    {
        Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var tsc = new TaskCompletionSource<bool>();
        _app.Lifetime.ApplicationStarted.Register(() => tsc.SetResult(true));
        _ = _app.RunAsync();
        await tsc.Task;
    }

    public async Task StopAsync()
    {
        await _app.StopAsync();
    }

}



