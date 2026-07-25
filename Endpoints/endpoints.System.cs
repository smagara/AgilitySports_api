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
        var legacyApi = routes.MapGroup("api");
        var v2Api = routes.MapGroup("api/v2");

        static IResult GetVersion(IConfiguration config)
        {
            var version = config.GetValue<string?>("Version") ?? "";
            return Results.Ok(version);
        }

        static IResult GetHealth(ILogger<Version> logger)
        {
            var reply = "AgilitySportsAPI is healthy: " + DateTime.Now.ToString();
            logger.LogInformation(reply);
            return Results.Ok(reply);
        }

        legacyApi.MapGet("version", (ILogger<Version> logger) =>
        {
            return GetVersion(configuration);
        });

        v2Api.MapGet("version", (ILogger<Version> logger) =>
        {
            return GetVersion(configuration);
        });

        legacyApi.MapGet("checkhealth", (ILogger<Version> logger) =>
        {
            return GetHealth(logger);
        });

        v2Api.MapGet("checkhealth", (ILogger<Version> logger) =>
        {
            return GetHealth(logger);
        });
    }
}