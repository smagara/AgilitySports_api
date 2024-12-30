// This file contains the endpoints for NBA-related operations such as CRUD operations on the NBA roster.

using AgilitySportsAPI.Data;
using AgilitySportsAPI.Models;

public static class NbaEndpoints
{
    /// <summary>
    /// Maps the NBA-related endpoints such as CRUD operations on the NBA roster.
    /// </summary>
    /// <param name="routes">The endpoint route builder.</param>
    public static void MapNbaEndpoints(this IEndpointRouteBuilder routes)
    {
        var NBA = routes.MapGroup("api/nba");

        NBA.MapGet("roster", async (ILogger<NBARoster> logger, int? playerId, INBARepo repo) =>
        {
            var results = await repo.GetNBARoster(logger, playerId);
            if (results != null)
            {
                return Results.Ok(results);
            }
            else
            {
                return Results.Problem("Error fetching NBA Roster, ask your admin to check the logs.");
            }
        });

        NBA.MapPost("roster", async (ILogger<NBARoster> logger, INBARepo repo, NBARoster roster) =>
        {
            NBARoster? newPlayer = await repo.CreateNBARoster(roster, logger);

            if (newPlayer != null)
            {
                return Results.Ok("Added to NBA Roster.");
            }
            else
            {
                return Results.Problem("Error adding to NBA Roster, check the logs.");
            }
        });

        NBA.MapPut("roster", async (ILogger<NBARoster> logger, INBARepo repo, NBARoster roster) =>
        {
            bool ret = await repo.UpdateNBARoster(roster, logger);

            if (ret == true)
            {
                return Results.Ok("Updated NBA Roster.");
            }
            else
            {
                return Results.Problem("Error updating the NBA Roster, check the logs.");
            }
        });

        NBA.MapDelete("roster", async (ILogger<NBARoster> logger, INBARepo repo, int playerId) =>
        {
            bool ret = await repo.DeleteNBARoster(playerId, logger);

            if (ret == true)
            {
                return Results.Ok("Deleted from NBA Roster.");
            }
            else
            {
                return Results.Problem("Error deleting from NBA Roster, check the logs.");
            }
        });
    }
}