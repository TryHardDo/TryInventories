using Serilog;
using TryInventories.Middlewares;
using TryInventories.Settings;
using TryInventories.Updater;

namespace TryInventories;

internal class TryInventories
{
    internal const string Author = "Levai Levente @ TryHardDo";
    internal static readonly Version Version = new(1, 1, 3);

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

        Log.Logger = logger;

        logger.Information("TryInventories - API provider");
        logger.Information("Developed by: {Author} | Version: {Version}", Author, Version);
        logger.Information("Project's source: https://github.com/TryHardDo/TryInventories");

        builder.Services.Configure<AppOptions>(builder.Configuration.GetSection(AppOptions.Settings));
        builder.Services.AddHostedService<VersionChecker>();
        builder.Services.AddHostedService<SteamProxy>();

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

        app.Run();
    }
}

// Todo: Adding sorting for proxy pool based on proxy response time (ping).