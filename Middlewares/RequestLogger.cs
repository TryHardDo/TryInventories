using Microsoft.AspNetCore.Http.Extensions;

namespace TryInventories.Middlewares;

// You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
public class RequestLogger
{
    private readonly ILogger<RequestLogger> _logger;
    private readonly RequestDelegate _next;

    public RequestLogger(RequestDelegate next, ILogger<RequestLogger> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
        _logger.LogInformation("Incoming request: {Method} {Path} from IP: {IP}",
            httpContext.Request.Method, httpContext.Request.GetDisplayUrl(), ipAddress);

        await _next(httpContext);
    }
}

// Extension method used to add the middleware to the HTTP request pipeline.
public static class RequestLoggerExtensions
{
    public static IApplicationBuilder UseRequestLogger(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLogger>();
    }
}