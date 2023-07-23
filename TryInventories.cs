using Serilog;
using TryInventories.Middlewares;
using TryInventories.Services;
using TryInventories.Settings;

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
        Log.Information("\n\n" +
                        "DISCLAIMER: Please be advised that the use of this program is at your own risk. The author of this program shall not be held responsible for any consequences that may arise from its usage.\n" +
                        "By using this program, you acknowledge that you have read and understood this disclaimer and agree to use the program at your own risk. Press any key to continue using the software." +
                        "\n");

        Console.ReadKey();
        Log.Information("Starting software...");

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(Log.Logger);

        builder.Services.Configure<AppOptions>(builder.Configuration.GetSection(AppOptions.Settings));
        builder.Services.AddHostedService<VersionChecker>();
        builder.Services.AddSingleton<SteamProxy>();
        builder.Services.PostConfigure<SteamProxy>(sp => { sp.Init(); });

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