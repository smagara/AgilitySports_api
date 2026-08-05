// This file contains the endpoints for PGA Tour roster CRUD.

using AgilitySportsAPI.Data;
using AgilitySportsAPI.Dtos;
using AgilitySportsAPI.Models;
using AgilitySportsAPI.Services;

public static class PgaEndpoints
{
    /// <summary>
    /// Maps the PGA-related endpoints such as CRUD operations on the PGA Tour roster.
    /// </summary>
    public static void MapPgaEndpoints(this IEndpointRouteBuilder routes)
    {
        void MapPgaRoutes(RouteGroupBuilder pga)
        {
            pga.MapGet("roster", async (ILogger<PGARoster> logger, int? playerId, IPGARepo repo) =>
            {
                var results = await repo.GetPGARoster(logger, playerId);
                if (results != null)
                {
                    return Results.Ok(results);
                }

                return Results.Problem("Error fetching PGA Roster, ask your admin to check the logs.");
            });

            pga.MapPost("roster", async (
                ILogger<PGARoster> logger,
                IPlayersV2WriteService writeService,
                IXssValidationService xssValidator,
                IInputSanitizationService sanitizer,
                IInputValidationService validator,
                PGARoster roster) =>
            {
                var (isValid, violations) = xssValidator.ValidateTextFields(roster, logger);
                if (!isValid)
                {
                    logger.LogWarning("XSS attempt blocked in PGA roster creation. Violations: {Violations}", string.Join(", ", violations));
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
                    logger.LogWarning("Validation errors in PGA roster creation. Errors: {Errors}", string.Join(", ", validationErrors));
                    return Results.BadRequest(new
                    {
                        Error = "Validation failed",
                        Message = "The request contains invalid data for structured fields.",
                        Details = validationErrors
                    });
                }

                var (statsValid, statsErrors) = ValidatePgaStats(roster);
                if (!statsValid)
                {
                    return Results.BadRequest(new
                    {
                        Error = "Validation failed",
                        Message = "The request contains invalid PGA stats.",
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
                    return Results.Ok("Added to PGA Roster.");
                }

                return Results.Problem("Error adding to PGA Roster, check the logs.");
            });

            pga.MapPut("roster", async (
                ILogger<PGARoster> logger,
                IPlayersV2WriteService writeService,
                IXssValidationService xssValidator,
                IInputSanitizationService sanitizer,
                IInputValidationService validator,
                PGARoster roster) =>
            {
                var (isValid, violations) = xssValidator.ValidateTextFields(roster, logger);
                if (!isValid)
                {
                    logger.LogWarning("XSS attempt blocked in PGA roster update. Violations: {Violations}", string.Join(", ", violations));
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
                    logger.LogWarning("Validation errors in PGA roster update. Errors: {Errors}", string.Join(", ", validationErrors));
                    return Results.BadRequest(new
                    {
                        Error = "Validation failed",
                        Message = "The request contains invalid data for structured fields.",
                        Details = validationErrors
                    });
                }

                var (statsValid, statsErrors) = ValidatePgaStats(roster);
                if (!statsValid)
                {
                    return Results.BadRequest(new
                    {
                        Error = "Validation failed",
                        Message = "The request contains invalid PGA stats.",
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
                    return Results.Ok("Updated PGA Roster.");
                }

                if (updateOutcome == PlayerWriteOutcome.NotFound)
                {
                    return Results.NotFound("PGA player was not found or update failed.");
                }

                if (updateOutcome == PlayerWriteOutcome.InvalidTeam)
                {
                    return Results.BadRequest(new
                    {
                        Error = "Validation failed",
                        Message = "Team code could not be resolved for PGA."
                    });
                }

                if (updateOutcome == PlayerWriteOutcome.StatsFailed)
                {
                    return Results.Problem("PGA player update failed while writing stats. Check logs for details.");
                }

                return Results.Problem("Error updating PGA Roster, check the logs.");
            });

            pga.MapDelete("roster", async (ILogger<PGARoster> logger, IPlayersV2WriteService writeService, int playerId) =>
            {
                var deleted = await writeService.DeletePlayer(logger, playerId);

                if (deleted)
                {
                    return Results.Ok("Deleted from PGA Roster.");
                }

                return Results.NotFound("PGA player was not found or delete failed.");
            });
        }

        MapPgaRoutes(routes.MapGroup("api/v2/pga"));
    }

    private static PlayerUpsertDto MapToPlayerUpsert(PGARoster roster) => new()
    {
        SportCode = "PGA",
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

    private static PlayerStatsUpsertDto MapToStatsUpsert(PGARoster roster) => new()
    {
        Wins = roster.Wins,
        Majors = roster.Majors,
        DrivingDistance = roster.DrivingDistance,
        ScoringAverage = roster.ScoringAverage,
        EventsPlayed = roster.EventsPlayed,
        CutsMade = roster.CutsMade
    };

    private static bool HasStats(PGARoster roster) =>
        roster.Wins.HasValue
        || roster.Majors.HasValue
        || roster.DrivingDistance.HasValue
        || roster.ScoringAverage.HasValue
        || roster.EventsPlayed.HasValue
        || roster.CutsMade.HasValue;

    private static (bool IsValid, List<string> Errors) ValidatePgaStats(PGARoster roster)
    {
        var errors = new List<string>();
        if (roster.Wins.HasValue && roster.Wins.Value < 0)
        {
            errors.Add("Wins must be greater than or equal to 0.");
        }

        if (roster.Majors.HasValue && roster.Majors.Value < 0)
        {
            errors.Add("Majors must be greater than or equal to 0.");
        }

        if (roster.DrivingDistance.HasValue && roster.DrivingDistance.Value < 0)
        {
            errors.Add("DrivingDistance must be greater than or equal to 0.");
        }

        if (roster.ScoringAverage.HasValue && roster.ScoringAverage.Value < 0)
        {
            errors.Add("ScoringAverage must be greater than or equal to 0.");
        }

        if (roster.EventsPlayed.HasValue && roster.EventsPlayed.Value < 0)
        {
            errors.Add("EventsPlayed must be greater than or equal to 0.");
        }

        if (roster.CutsMade.HasValue && roster.CutsMade.Value < 0)
        {
            errors.Add("CutsMade must be greater than or equal to 0.");
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
