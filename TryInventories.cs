using Serilog;
using TryInventories.Middlewares;
using TryInventories.Services;
using TryInventories.SettingModels;

namespace TryInventories;

internal class TryInventories
{
    internal const string Author = "Levai Levente @ TryHardDo";
    internal static readonly Version Version = new(1, 1, 3);

    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .WriteTo.Console()
            .Enrich.FromLogContext()
            .CreateLogger();

        Log.Information("TryInventories - Steam inventory loader");
        Log.Information("Developed by: {Author} | Version: {Version}", Author, Version);
        Log.Information("Starting software...");

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(Log.Logger);

        builder.Services.Configure<Settings>(builder.Configuration.GetSection(Settings.SectionName));
        builder.Services.AddSingleton<SteamProxy>();

        builder.Services.AddHostedService<VersionChecker>();
        builder.Services.AddHostedService<ProxyUpdater>();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        app.Services.GetRequiredService<SteamProxy>().Init();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseRequestLogger();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}