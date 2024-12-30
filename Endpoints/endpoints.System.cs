// This file contains the endpoints for system-related operations such as version and health check.

using AgilitySportsAPI.Data;
using AgilitySportsAPI.Models;

public static class SystemEndpoints
{
    /// <summary>
    /// Maps the system-related endpoints such as version and health check.
    /// </summary>
    /// <param name="routes">The endpoint route builder.</param>
    /// <param name="configuration">The application configuration (appsettings.json).</param>
    public static void MapSystemEndpoints(this IEndpointRouteBuilder routes, IConfiguration configuration)
    {
        var API = routes.MapGroup("api");
        API.MapGet("version", (ILogger<Version> logger) =>
        {
            var version = configuration.GetValue<string?>("Version") ?? "";
            return Results.Ok(version);
        });

        API.MapGet("checkhealth", (ILogger<Version> logger) =>
        {
            var reply = "AgilitySportsAPI is healthy: " + DateTime.Now.ToString();
            logger.LogInformation(reply);
            return Results.Ok(reply);
        });
    }
}