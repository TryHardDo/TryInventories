using Microsoft.Extensions.Options;
using Serilog;
using TryInventories.Middlewares;
using TryInventories.Settings;
using TryInventories.Updater;
using TryInventories.WebShareApi;

namespace TryInventories;

internal class TryInventories
{
    internal const string Author = "Levai Levente @ TryHardDo";
    internal static readonly Version Version = new(1, 1, 0);

    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .WriteTo.Console()
            .Enrich.FromLogContext()
            .CreateLogger();

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(logger);

        logger.Information("TryInventories - API provider");
        logger.Information("Developed by: {Author} | Version: {Version}", Author, Version);
        logger.Information("Project's source: https://github.com/TryHardDo/TryInventories");
        builder.Services.Configure<AppOptions>(builder.Configuration.GetSection(AppOptions.Settings));

        // WebShare client singleton service
        builder.Services.AddSingleton(sp =>
        {
            var configuration = sp.GetRequiredService<IOptions<AppOptions>>();
            var wc = new WebShareClient(configuration.Value.SelfRotatedProxySettings.WebShareApiKey);

            return wc;
        });


        // Version checker singleton service
        builder.Services.AddSingleton(sp =>
        {
            var loggerService = sp.GetRequiredService<ILogger<VersionChecker>>();
            var vc = new VersionChecker(loggerService);
            vc.StartScheduledVersionChecker();

            return vc;
        });

        // SteamProxy request middleman singleton service
        builder.Services.AddSingleton(sp =>
        {
            var configuration = sp.GetRequiredService<IOptions<AppOptions>>();
            var loggerService = sp.GetRequiredService<ILogger<SteamProxy>>();

            var proxy = new SteamProxy(loggerService, configuration);
            proxy.Init();

            return proxy;
        });

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseRequestLogger();

        //app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        // Resolving services early to run init methods before app.Run()
        app.Services.GetService<VersionChecker>();
        app.Services.GetService<SteamProxy>();

        app.Run();
    }
}