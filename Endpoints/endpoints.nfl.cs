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
        
        // Read
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

        // Create
        NFL.MapPost("roster", async (ILogger<NFLRoster> logger, INFLRepo repo, NFLRoster roster) =>
        {
            NFLRoster? newPlayer = await repo.Create(roster, logger);

            if (newPlayer != null)
            {
                return Results.Ok("Added to NFL Roster.");
            }
            else
            {
                return Results.Problem("Error adding to NFL Roster, check the logs.");
            }
        });

        // Update
        NFL.MapPut("roster", async (ILogger<NFLRoster> logger, INFLRepo repo, NFLRoster roster) =>
        {
            bool ret = await repo.Update(roster, logger);

            if (ret == true)
            {
                return Results.Ok("Updated NFL Roster.");
            }
            else
            {
                return Results.Problem("Error updating the NFL Roster, check the logs.");
            }
        });

        // Delete
        NFL.MapDelete("roster", async (ILogger<NFLRoster> logger, INFLRepo repo, int playerId) =>
        {
            bool ret = await repo.Delete(playerId, logger);

            if (ret == true)
            {
                return Results.Ok("Deleted from NFL Roster.");
            }
            else
            {
                return Results.Problem("Error deleting from NFL Roster, check the logs.");
            }
        });
    }
}