// This file contains the endpoints for NFL-related operations such as fetching the NFL roster.

using AgilitySportsAPI.Data;
using AgilitySportsAPI.Models;
using AgilitySportsAPI.Services;

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
        NFL.MapPost("roster", async (ILogger<NFLRoster> logger, INFLRepo repo, IXssValidationService xssValidator, IInputSanitizationService sanitizer, IInputValidationService validator,  NFLRoster roster) =>
        {
            // Validate for XSS patterns
            var (isValid, violations) = xssValidator.ValidateTextFields(roster, logger);
            
            if (!isValid)
            {
                logger.LogWarning("XSS attempt blocked in NFL roster creation. Violations: {Violations}", string.Join(", ", violations));
                return Results.BadRequest(new 
                { 
                    Error = "XSS attempt detected", 
                    Message = "The request contains potentially malicious content and has been blocked for security reasons.",
                    Details = violations
                });
            }

            // Validate structured fields (height, weight, age, position, etc.)
            var (isValidStructured, validationErrors) = validator.ValidateModel(roster, logger);
            
            if (!isValidStructured)
            {
                logger.LogWarning("Validation errors in NFL roster creation. Errors: {Errors}", string.Join(", ", validationErrors));
                return Results.BadRequest(new 
                { 
                    Error = "Validation failed", 
                    Message = "The request contains invalid data for structured fields.",
                    Details = validationErrors
                });
            }

            // Sanitize input after all validation passes
            var sanitizedRoster = sanitizer.SanitizeModel(roster, logger);

            NFLRoster? newPlayer = await repo.Create(sanitizedRoster, logger);

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
        NFL.MapPut("roster", async (ILogger<NFLRoster> logger, INFLRepo repo, IXssValidationService xssValidator, IInputSanitizationService sanitizer, IInputValidationService validator, NFLRoster roster) =>
        {
            // Validate for XSS patterns
            var (isValid, violations) = xssValidator.ValidateTextFields(roster, logger);
            
            if (!isValid)
            {
                logger.LogWarning("XSS attempt blocked in NFL roster update. Violations: {Violations}", string.Join(", ", violations));
                return Results.BadRequest(new 
                { 
                    Error = "XSS attempt detected", 
                    Message = "The request contains potentially malicious content and has been blocked for security reasons.",
                    Details = violations
                });
            }

            // Validate structured fields (height, weight, age, position, etc.)
            var (isValidStructured, validationErrors) = validator.ValidateModel(roster, logger);
            
            if (!isValidStructured)
            {
                logger.LogWarning("Validation errors in NFL roster update. Errors: {Errors}", string.Join(", ", validationErrors));
                return Results.BadRequest(new 
                { 
                    Error = "Validation failed", 
                    Message = "The request contains invalid data for structured fields.",
                    Details = validationErrors
                });
            }

            // Sanitize input after all validation passes
            var sanitizedRoster = sanitizer.SanitizeModel(roster, logger);

            bool ret = await repo.Update(sanitizedRoster, logger);

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