using Serilog;
using TryInventories.Middlewares;
using TryInventories.Settings;

namespace TryInventories;

internal class Program
{
    internal const string Version = "v1.0.0";
    internal const string Author = "Levai Levente @ TryHardDo";

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

        builder.Services.AddSingleton<SteamProxy>();

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

        app.Services.GetService<SteamProxy>().Init(); // Other solution?

        app.Run();
    }
}