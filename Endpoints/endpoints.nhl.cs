// This file contains the endpoints for NHL-related operations such as CRUD operations on the NHL roster.

using AgilitySportsAPI.Data;
using AgilitySportsAPI.Models;
using AgilitySportsAPI.Services;

public static class NhlEndpoints
{
    /// <summary>
    /// Maps the NHL-related endpoints such as CRUD operations on the NHL roster.
    /// </summary>
    /// <param name="routes">The endpoint route builder.</param>
    public static void MapNhlEndpoints(this IEndpointRouteBuilder routes)
    {
        var NHL = routes.MapGroup("api/nhl");

        // Read
        NHL.MapGet("roster", async (ILogger<NHLRoster> logger, int? playerId, INHLRepo repo) =>
        {
            var results = await repo.GetNHLRoster(logger, playerId);
            if (results != null)
            {
                return Results.Ok(results);
            }
            else
            {
                return Results.Problem("Error fetching NHL Roster, ask your admin to check the logs.");
            }
        });

        // Create
        NHL.MapPost("roster", async (ILogger<NHLRoster> logger, INHLRepo repo, IXssValidationService xssValidator, NHLRoster roster) =>
        {
            // Validate for XSS patterns
            var (isValid, violations) = xssValidator.ValidateTextFields(roster, logger);
            
            if (!isValid)
            {
                logger.LogWarning("XSS attempt blocked in NHL roster creation. Violations: {Violations}", string.Join(", ", violations));
                return Results.BadRequest(new 
                { 
                    Error = "XSS attempt detected", 
                    Message = "The request contains potentially malicious content and has been blocked for security reasons.",
                    Details = violations
                });
            }

            NHLRoster? newPlayer = await repo.CreateNHLRoster(roster, logger);

            if (newPlayer != null)
            {
                return Results.Ok("Added to NHL Roster.");
            }
            else
            {
                return Results.Problem("Error adding to NHL Roster, check the logs.");
            }
        });

        // Update
        NHL.MapPut("roster", async (ILogger<NHLRoster> logger, INHLRepo repo, IXssValidationService xssValidator, NHLRoster roster) =>
        {
            // Validate for XSS patterns
            var (isValid, violations) = xssValidator.ValidateTextFields(roster, logger);
            
            if (!isValid)
            {
                logger.LogWarning("XSS attempt blocked in NHL roster update. Violations: {Violations}", string.Join(", ", violations));
                return Results.BadRequest(new 
                { 
                    Error = "XSS attempt detected", 
                    Message = "The request contains potentially malicious content and has been blocked for security reasons.",
                    Details = violations
                });
            }

            bool ret = await repo.UpdateNHLRoster(roster, logger);

            if (ret == true)
            {
                return Results.Ok("Updated NHL Roster.");
            }
            else
            {
                return Results.Problem("Error updating the NHL Roster, check the logs.");
            }
        });

        // Delete
        NHL.MapDelete("roster", async (ILogger<NHLRoster> logger, INHLRepo repo, int playerId) =>
        {

            bool ret = await repo.DeleteNHLRoster(playerId, logger);

            if (ret == true)
            {
                return Results.Ok("Deleted from NHL Roster.");
            }
            else
            {
                return Results.Problem("Error deleting from NHL Roster, check the logs.");

            }
        });
    }
}