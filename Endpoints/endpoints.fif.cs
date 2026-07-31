// This file contains the endpoints for FIFA World Cup (FIF) roster CRUD.

using AgilitySportsAPI.Data;
using AgilitySportsAPI.Dtos;
using AgilitySportsAPI.Models;
using AgilitySportsAPI.Services;

public static class FifEndpoints
{
    /// <summary>
    /// Maps the FIF-related endpoints such as CRUD operations on the FIFA World Cup roster.
    /// </summary>
    public static void MapFifEndpoints(this IEndpointRouteBuilder routes)
    {
        void MapFifRoutes(RouteGroupBuilder fif)
        {
            fif.MapGet("roster", async (ILogger<FIFRoster> logger, int? playerId, IFIFRepo repo) =>
            {
                var results = await repo.GetFIFRoster(logger, playerId);
                if (results != null)
                {
                    return Results.Ok(results);
                }

                return Results.Problem("Error fetching FIF Roster, ask your admin to check the logs.");
            });

            fif.MapPost("roster", async (
                ILogger<FIFRoster> logger,
                IPlayersV2WriteService writeService,
                IXssValidationService xssValidator,
                IInputSanitizationService sanitizer,
                IInputValidationService validator,
                FIFRoster roster) =>
            {
                var (isValid, violations) = xssValidator.ValidateTextFields(roster, logger);
                if (!isValid)
                {
                    logger.LogWarning("XSS attempt blocked in FIF roster creation. Violations: {Violations}", string.Join(", ", violations));
                    return Results.BadRequest(new
                    {
                        Error = "XSS attempt detected",
                        Message = "The request contains potentially malicious content and has been blocked for security reasons.",
                        Details = violations
                    });
                }

                var (isValidStructured, validationErrors) = validator.ValidateModel(roster, logger);
                if (!isValidStructured)
                {
                    logger.LogWarning("Validation errors in FIF roster creation. Errors: {Errors}", string.Join(", ", validationErrors));
                    return Results.BadRequest(new
                    {
                        Error = "Validation failed",
                        Message = "The request contains invalid data for structured fields.",
                        Details = validationErrors
                    });
                }

                var (statsValid, statsErrors) = ValidateFifStats(roster);
                if (!statsValid)
                {
                    return Results.BadRequest(new
                    {
                        Error = "Validation failed",
                        Message = "The request contains invalid FIF stats.",
                        Details = statsErrors
                    });
                }

                var sanitizedRoster = sanitizer.SanitizeModel(roster, logger);
                var player = MapToPlayerUpsert(sanitizedRoster);

                if (string.IsNullOrWhiteSpace(player.TeamCode))
                {
                    return Results.BadRequest(new
                    {
                        Error = "Validation failed",
                        Message = "Team code is required."
                    });
                }

                var hasStats = HasStats(sanitizedRoster);
                var createdId = hasStats
                    ? await writeService.CreatePlayerWithStats(logger, player, MapToStatsUpsert(sanitizedRoster))
                    : await writeService.CreatePlayer(logger, player);

                if (createdId != null)
                {
                    return Results.Ok("Added to FIF Roster.");
                }

                return Results.Problem("Error adding to FIF Roster, check the logs.");
            });

            fif.MapPut("roster", async (
                ILogger<FIFRoster> logger,
                IPlayersV2WriteService writeService,
                IXssValidationService xssValidator,
                IInputSanitizationService sanitizer,
                IInputValidationService validator,
                FIFRoster roster) =>
            {
                var (isValid, violations) = xssValidator.ValidateTextFields(roster, logger);
                if (!isValid)
                {
                    logger.LogWarning("XSS attempt blocked in FIF roster update. Violations: {Violations}", string.Join(", ", violations));
                    return Results.BadRequest(new
                    {
                        Error = "XSS attempt detected",
                        Message = "The request contains potentially malicious content and has been blocked for security reasons.",
                        Details = violations
                    });
                }

                var (isValidStructured, validationErrors) = validator.ValidateModel(roster, logger);
                if (!isValidStructured)
                {
                    logger.LogWarning("Validation errors in FIF roster update. Errors: {Errors}", string.Join(", ", validationErrors));
                    return Results.BadRequest(new
                    {
                        Error = "Validation failed",
                        Message = "The request contains invalid data for structured fields.",
                        Details = validationErrors
                    });
                }

                var (statsValid, statsErrors) = ValidateFifStats(roster);
                if (!statsValid)
                {
                    return Results.BadRequest(new
                    {
                        Error = "Validation failed",
                        Message = "The request contains invalid FIF stats.",
                        Details = statsErrors
                    });
                }

                var sanitizedRoster = sanitizer.SanitizeModel(roster, logger);
                var player = MapToPlayerUpsert(sanitizedRoster);

                if (string.IsNullOrWhiteSpace(player.TeamCode))
                {
                    return Results.BadRequest(new
                    {
                        Error = "Validation failed",
                        Message = "Team code is required."
                    });
                }

                var hasStats = HasStats(sanitizedRoster);
                var updateOutcome = hasStats
                    ? await writeService.UpdatePlayerWithStatsDetailed(logger, sanitizedRoster.PlayerId, player, MapToStatsUpsert(sanitizedRoster))
                    : await writeService.UpdatePlayerDetailed(logger, sanitizedRoster.PlayerId, player);

                if (updateOutcome == PlayerWriteOutcome.Success)
                {
                    return Results.Ok("Updated FIF Roster.");
                }

                if (updateOutcome == PlayerWriteOutcome.NotFound)
                {
                    return Results.NotFound("FIF player was not found or update failed.");
                }

                if (updateOutcome == PlayerWriteOutcome.InvalidTeam)
                {
                    return Results.BadRequest(new
                    {
                        Error = "Validation failed",
                        Message = "Team code could not be resolved for FIF."
                    });
                }

                if (updateOutcome == PlayerWriteOutcome.StatsFailed)
                {
                    return Results.Problem("FIF player update failed while writing stats. Check logs for details.");
                }

                return Results.Problem("Error updating FIF Roster, check the logs.");
            });

            fif.MapDelete("roster", async (ILogger<FIFRoster> logger, IPlayersV2WriteService writeService, int playerId) =>
            {
                var deleted = await writeService.DeletePlayer(logger, playerId);

                if (deleted)
                {
                    return Results.Ok("Deleted from FIF Roster.");
                }

                return Results.NotFound("FIF player was not found or delete failed.");
            });
        }

        MapFifRoutes(routes.MapGroup("api/v2/fif"));
    }

    private static PlayerUpsertDto MapToPlayerUpsert(FIFRoster roster) => new()
    {
        SportCode = "FIF",
        TeamCode = roster.TeamCode?.Trim() ?? roster.Team?.Trim() ?? string.Empty,
        PositionCode = roster.Position?.Trim(),
        FirstName = roster.FirstName,
        LastName = roster.LastName,
        DateOfBirth = roster.DateOfBirth,
        Height = roster.Height,
        Weight = ParseNullableInt(roster.Weight),
        Number = ParseNullableInt(roster.Number),
        College = roster.College,
        BirthCityState = roster.BirthCityState,
        BirthCountry = roster.BirthCountry,
        DraftYear = roster.DraftYear,
        SeasonYear = roster.SeasonYear
    };

    private static PlayerStatsUpsertDto MapToStatsUpsert(FIFRoster roster) => new()
    {
        TotalGoals = roster.TotalGoals,
        Assists = roster.Assists,
        Saves = roster.Saves
    };

    private static bool HasStats(FIFRoster roster) =>
        roster.TotalGoals.HasValue || roster.Assists.HasValue || roster.Saves.HasValue;

    private static (bool IsValid, List<string> Errors) ValidateFifStats(FIFRoster roster)
    {
        var errors = new List<string>();
        if (roster.TotalGoals.HasValue && roster.TotalGoals.Value < 0)
        {
            errors.Add("TotalGoals must be greater than or equal to 0.");
        }

        if (roster.Assists.HasValue && roster.Assists.Value < 0)
        {
            errors.Add("Assists must be greater than or equal to 0.");
        }

        if (roster.Saves.HasValue && roster.Saves.Value < 0)
        {
            errors.Add("Saves must be greater than or equal to 0.");
        }

        return (errors.Count == 0, errors);
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
