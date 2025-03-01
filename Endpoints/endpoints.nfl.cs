// This file contains the endpoints for NFL-related operations such as fetching the NFL roster.

using AgilitySportsAPI.Data;
using AgilitySportsAPI.Models;

public static class NflEndpoints
{
    /// <summary>
    /// Maps the NFL-related endpoints such as fetching the NFL roster.
    /// </summary>
    /// <param name="routes">The endpoint route builder.</param>
    public static void MapNflEndpoints(this IEndpointRouteBuilder routes)
    {
        var NFL = routes.MapGroup("api/nfl");
        
        NFL.MapGet("roster", async (ILogger<NFLRoster> logger, int? playerId, INFLRepo repo) =>
        {
            var results = await repo.GetNFLRoster(logger, playerId);
            if (results != null)
            {
                return Results.Ok(results);
            }
            else
            {
                return Results.Problem("Error fetching NFL Roster, ask your admin to check the logs.");
            }
        });
    }
}