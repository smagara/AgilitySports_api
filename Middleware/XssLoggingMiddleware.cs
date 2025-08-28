using AgilitySportsAPI.Services;

namespace AgilitySportsAPI.Middleware;

public class XssLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<XssLoggingMiddleware> _logger;

    public XssLoggingMiddleware(RequestDelegate next, ILogger<XssLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Log the request details for potential XSS attempts
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

        // Log all requests for monitoring (you might want to adjust the log level)
        _logger.LogInformation("Request received: {RequestInfo}", requestInfo);

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
