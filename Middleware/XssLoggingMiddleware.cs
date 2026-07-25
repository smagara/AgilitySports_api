using AgilitySportsAPI.Services;

namespace AgilitySportsAPI.Middleware;

public class XssLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<XssLoggingMiddleware> _logger;
    private readonly bool _enableRequestLogging;

    public XssLoggingMiddleware(RequestDelegate next, ILogger<XssLoggingMiddleware> logger, IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _enableRequestLogging = configuration.GetValue<bool>("XssLogging:EnableRequestLogging", false);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (_enableRequestLogging)
        {
            // Log request metadata when explicitly enabled for diagnostics.
            var requestInfo = new
            {
                Timestamp = DateTime.UtcNow,
                IPAddress = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.Request.Headers["User-Agent"].ToString(),
                Method = context.Request.Method,
                Path = context.Request.Path,
                QueryString = context.Request.QueryString.ToString(),
                Headers = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
            };

            _logger.LogInformation("Request received: {RequestInfo}", requestInfo);
        }

        await _next(context);
    }
}

public static class XssLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseXssLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<XssLoggingMiddleware>();
    }
}
