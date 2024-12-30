// This file contains the endpoints for static data operations such as fetching position codes.

using AgilitySportsAPI.Data;
using AgilitySportsAPI.Models;

public static class StaticDataEndpoints
{
    /// <summary>
    /// Maps the static data-related endpoints such as fetching position codes.
    /// </summary>
    /// <param name="routes">The endpoint route builder.</param>
    public static void MapStaticDataEndpoints(this IEndpointRouteBuilder routes)
    {
        var staticData = routes.MapGroup("api/staticdata");
        staticData.MapGet("positions", async (ILogger<PositionCodes> logger, IStaticData repoPosition, string sport) =>
        {
            var results = await repoPosition.GetPositionCodes(logger, sport);
            if (results != null)
            {
                return Results.Ok(results);
            }
            else
            {
                return Results.Problem("Error fetching sport Positions for " + sport + ", ask your admin to check the logs.");
            }
        });
    }
}