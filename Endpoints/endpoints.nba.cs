// This file contains the endpoints for NBA-related operations such as CRUD operations on the NBA roster.

using AgilitySportsAPI.Data;
using AgilitySportsAPI.Dtos;
using AgilitySportsAPI.Models;
using AgilitySportsAPI.Services;

public static class NbaEndpoints
{
    /// <summary>
    /// Maps the NBA-related endpoints such as CRUD operations on the NBA roster.
    /// </summary>
    /// <param name="routes">The endpoint route builder.</param>
    public static void MapNbaEndpoints(this IEndpointRouteBuilder routes)
    {
        void MapNbaRoutes(RouteGroupBuilder nba)
        {
            nba.MapGet("roster", async (ILogger<NBARoster> logger, int? playerId, INBARepo repo) =>
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

            nba.MapPost("roster", async (ILogger<NBARoster> logger, IPlayersV2WriteService writeService, IXssValidationService xssValidator, IInputSanitizationService sanitizer, IInputValidationService validator, NBARoster roster) =>
            {
                // Validate for XSS patterns
                var (isValid, violations) = xssValidator.ValidateTextFields(roster, logger);

                if (!isValid)
                { 
                    logger.LogWarning("XSS attempt blocked in NBA roster creation. Violations: {Violations}", string.Join(", ", violations));
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
                    logger.LogWarning("Validation errors in NBA roster creation. Errors: {Errors}", string.Join(", ", validationErrors));
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
                    SportCode = "NBA",
                    TeamCode = sanitizedRoster.TeamCode?.Trim() ?? sanitizedRoster.Team?.Trim() ?? string.Empty,
                    PositionCode = sanitizedRoster.Position?.Trim(),
                    FirstName = sanitizedRoster.FirstName,
                    LastName = sanitizedRoster.LastName,
                    DateOfBirth = sanitizedRoster.DateOfBirth,
                    Height = sanitizedRoster.Height,
                    Weight = ParseNullableInt(sanitizedRoster.Weight),
                    Number = ParseNullableInt(sanitizedRoster.Number),
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

                var hasStats = sanitizedRoster.PointsPerGame.HasValue
                    || sanitizedRoster.ReboundsPerGame.HasValue
                    || sanitizedRoster.AssistsPerGame.HasValue;

                var createdId = hasStats
                    ? await writeService.CreatePlayerWithStats(logger, player, new PlayerStatsUpsertDto
                    {
                        PointsPerGame = sanitizedRoster.PointsPerGame,
                        ReboundsPerGame = sanitizedRoster.ReboundsPerGame,
                        AssistsPerGame = sanitizedRoster.AssistsPerGame
                    })
                    : await writeService.CreatePlayer(logger, player);

                if (createdId != null)
                { 
                    return Results.Ok("Added to NBA Roster.");
                }
                else
                { 
                    return Results.Problem("Error adding to NBA Roster, check the logs.");
                }
            });

            nba.MapPut("roster", async (ILogger<NBARoster> logger, IPlayersV2WriteService writeService, IXssValidationService xssValidator, IInputSanitizationService sanitizer, IInputValidationService validator, NBARoster roster) =>
            {
                // Validate for XSS patterns
                var (isValid, violations) = xssValidator.ValidateTextFields(roster, logger);

                if (!isValid)
                {
                    logger.LogWarning("XSS attempt blocked in NBA roster update. Violations: {Violations}", string.Join(", ", violations));
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
                    logger.LogWarning("Validation errors in NBA roster update. Errors: {Errors}", string.Join(", ", validationErrors));
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
                    SportCode = "NBA",
                    TeamCode = sanitizedRoster.TeamCode?.Trim() ?? sanitizedRoster.Team?.Trim() ?? string.Empty,
                    PositionCode = sanitizedRoster.Position?.Trim(),
                    FirstName = sanitizedRoster.FirstName,
                    LastName = sanitizedRoster.LastName,
                    DateOfBirth = sanitizedRoster.DateOfBirth,
                    Height = sanitizedRoster.Height,
                    Weight = ParseNullableInt(sanitizedRoster.Weight),
                    Number = ParseNullableInt(sanitizedRoster.Number),
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

                var hasStats = sanitizedRoster.PointsPerGame.HasValue
                    || sanitizedRoster.ReboundsPerGame.HasValue
                    || sanitizedRoster.AssistsPerGame.HasValue;

                var updateOutcome = hasStats
                    ? await writeService.UpdatePlayerWithStatsDetailed(logger, sanitizedRoster.PlayerId, player, new PlayerStatsUpsertDto
                    {
                        PointsPerGame = sanitizedRoster.PointsPerGame,
                        ReboundsPerGame = sanitizedRoster.ReboundsPerGame,
                        AssistsPerGame = sanitizedRoster.AssistsPerGame
                    })
                    : await writeService.UpdatePlayerDetailed(logger, sanitizedRoster.PlayerId, player);

                if (updateOutcome == PlayerWriteOutcome.Success)
                {
                    return Results.Ok("Updated NBA Roster.");
                }

                if (updateOutcome == PlayerWriteOutcome.NotFound)
                {
                    return Results.NotFound("NBA player was not found or update failed.");
                }

                if (updateOutcome == PlayerWriteOutcome.InvalidTeam)
                {
                    return Results.BadRequest(new
                    {
                        Error = "Validation failed",
                        Message = "Team code could not be resolved for NBA."
                    });
                }

                if (updateOutcome == PlayerWriteOutcome.StatsFailed)
                {
                    return Results.Problem("NBA player update failed while writing stats. Check logs for details.");
                }

                return Results.Problem("Error updating NBA Roster, check the logs.");
            });

            nba.MapDelete("roster", async (ILogger<NBARoster> logger, IPlayersV2WriteService writeService, int playerId) =>
            {
                var deleted = await writeService.DeletePlayer(logger, playerId);

                if (deleted)
                {
                    return Results.Ok("Deleted from NBA Roster.");
                }
                else
                {
                    return Results.NotFound("NBA player was not found or delete failed.");
                }
            });
        }

        MapNbaRoutes(routes.MapGroup("api/nba"));
        MapNbaRoutes(routes.MapGroup("api/v2/nba"));
    }

    private static int? ParseNullableInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return int.TryParse(value.Trim(), out var parsed) ? parsed : null;
    }
}