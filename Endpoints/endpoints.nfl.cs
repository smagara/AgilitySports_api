// This file contains the endpoints for NFL-related operations such as fetching the NFL roster.

using AgilitySportsAPI.Data;
using AgilitySportsAPI.Dtos;
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
        void MapNflRoutes(RouteGroupBuilder nfl)
        {
            // Read
            nfl.MapGet("roster", async (ILogger<NFLRoster> logger, int? playerId, INFLRepo repo) =>
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
            nfl.MapPost("roster", async (ILogger<NFLRoster> logger, IPlayersV2WriteService writeService, IXssValidationService xssValidator, IInputSanitizationService sanitizer, IInputValidationService validator, NFLRoster roster) =>
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

                var player = new PlayerUpsertDto
                {
                    SportCode = "NFL",
                    TeamCode = sanitizedRoster.TeamCode?.Trim() ?? sanitizedRoster.Team?.Trim() ?? string.Empty,
                    PositionCode = sanitizedRoster.Position?.Trim(),
                    FirstName = sanitizedRoster.FirstName,
                    LastName = sanitizedRoster.LastName,
                    DateOfBirth = sanitizedRoster.DateOfBirth,
                    Height = sanitizedRoster.Height,
                    Weight = sanitizedRoster.Weight,
                    Number = sanitizedRoster.Number,
                    College = sanitizedRoster.College,
                    BirthCityState = sanitizedRoster.BirthCityState,
                    BirthCountry = sanitizedRoster.BirthCountry,
                    DraftYear = sanitizedRoster.DraftYear,
                    SeasonYear = sanitizedRoster.SeasonYear
                };

                if (string.IsNullOrWhiteSpace(player.TeamCode))
                {
                    return Results.BadRequest(new
                    {
                        Error = "Validation failed",
                        Message = "Team code is required."
                    });
                }

                var createdId = await writeService.CreatePlayer(logger, player);

                if (createdId != null)
                { 
                    return Results.Ok("Added to NFL Roster.");
                }
                else
                { 
                    return Results.Problem("Error adding to NFL Roster, check the logs.");
                }
            });

            // Update
            nfl.MapPut("roster", async (ILogger<NFLRoster> logger, IPlayersV2WriteService writeService, IXssValidationService xssValidator, IInputSanitizationService sanitizer, IInputValidationService validator, NFLRoster roster) =>
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

                var player = new PlayerUpsertDto
                {
                    SportCode = "NFL",
                    TeamCode = sanitizedRoster.TeamCode?.Trim() ?? sanitizedRoster.Team?.Trim() ?? string.Empty,
                    PositionCode = sanitizedRoster.Position?.Trim(),
                    FirstName = sanitizedRoster.FirstName,
                    LastName = sanitizedRoster.LastName,
                    DateOfBirth = sanitizedRoster.DateOfBirth,
                    Height = sanitizedRoster.Height,
                    Weight = sanitizedRoster.Weight,
                    Number = sanitizedRoster.Number,
                    College = sanitizedRoster.College,
                    BirthCityState = sanitizedRoster.BirthCityState,
                    BirthCountry = sanitizedRoster.BirthCountry,
                    DraftYear = sanitizedRoster.DraftYear,
                    SeasonYear = sanitizedRoster.SeasonYear
                };

                if (string.IsNullOrWhiteSpace(player.TeamCode))
                {
                    return Results.BadRequest(new
                    {
                        Error = "Validation failed",
                        Message = "Team code is required."
                    });
                }

                var updated = await writeService.UpdatePlayer(logger, sanitizedRoster.PlayerId, player);

                if (updated)
                {
                    return Results.Ok("Updated NFL Roster.");
                }
                else
                {
                    return Results.NotFound("NFL player was not found or update failed.");
                }
            });

            // Delete
            nfl.MapDelete("roster", async (ILogger<NFLRoster> logger, IPlayersV2WriteService writeService, int playerId) =>
            {
                var deleted = await writeService.DeletePlayer(logger, playerId);

                if (deleted)
                {
                    return Results.Ok("Deleted from NFL Roster.");
                }
                else
                {
                    return Results.NotFound("NFL player was not found or delete failed.");
                }
            });
        }

        MapNflRoutes(routes.MapGroup("api/nfl"));
        MapNflRoutes(routes.MapGroup("api/v2/nfl"));
    }
}