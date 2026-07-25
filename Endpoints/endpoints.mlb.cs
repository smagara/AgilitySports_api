// This file contains the endpoints for MLB-related operations such as fetching rosters, attendance, and chart data.

using AgilitySportsAPI.Data;
using AgilitySportsAPI.Dtos;
using AgilitySportsAPI.Models;
using AgilitySportsAPI.Services;

public static class MlbDataEndpoints
{
    const short defaultChartYear = 2019;
    const short defaultDecadesBegin = 1920;
    const short defaultDecadesEnd = 2010;

    /// <summary>
    /// Maps the MLB-related endpoints such as fetching rosters, attendance, and chart data.
    /// </summary>
    /// <param name="routes">The endpoint route builder.</param>
    public static void MapMlbEndpoints(this IEndpointRouteBuilder routes)
    {
        void MapMlbRoutes(RouteGroupBuilder mlb)
        {
            // Endpoint to get all MLB rosters
            mlb.MapGet("roster/all", async (ILogger<MLBRoster> logger, IMLBRepo repoBaseball) =>
            {
                return Results.Ok(await repoBaseball.GetAllMLBRoster());
            });

            // Endpoint to get MLB roster with logging
            mlb.MapGet("roster", async (ILogger<MLBRoster> logger, IMLBRepo repoBaseball) =>
            {
                return Results.Ok(await repoBaseball.GetMLBRoster(logger));
            });

            // Endpoint to get all MLB attendance records
            mlb.MapGet("attendance/all", async (ILogger<MLBRoster> logger, IMLBRepo repoBaseball) =>
            {
                return Results.Ok(await repoBaseball.GetAllMLBAttendance());
            });

            // Endpoint to get MLB attendance records with optional year parameter
            mlb.MapGet("attendance", async (ILogger<MLBAttendanceDto> logger, short? yearId, IMLBRepo repoBaseball) =>
            {
                return Results.Ok(await repoBaseball.GetMLBAttendance(logger, yearId));
            });

            // Endpoint to get MLB chart data with optional year parameter
            mlb.MapGet("chart", async (ILogger<MLBAttendChartDTO> logger, short? yearId, IMLBRepo repoBaseball) =>
            {
                return Results.Ok(await repoBaseball.GetMLBChart(logger, yearId ?? defaultChartYear));
            });

            // Endpoint to get MLB decades data with optional begin and end decade parameters
            mlb.MapGet("decades", async (ILogger<MLBAttendChartDTO> logger, short? beginDecade, short? endDecade, IMLBRepo repoBaseball) =>
            {
                return Results.Ok(await repoBaseball.GetMLBDecades(logger, beginDecade ?? defaultDecadesBegin, endDecade ?? defaultDecadesEnd));
            });

            mlb.MapPost("roster", async (ILogger<MLBRoster> logger, IPlayersV2WriteService writeService, IXssValidationService xssValidator, IInputSanitizationService sanitizer, IInputValidationService validator, MLBRoster roster) =>
            {
                var (isValid, violations) = xssValidator.ValidateTextFields(roster, logger);
                if (!isValid)
                {
                    logger.LogWarning("XSS attempt blocked in MLB roster creation. Violations: {Violations}", string.Join(", ", violations));
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
                    logger.LogWarning("Validation errors in MLB roster creation. Errors: {Errors}", string.Join(", ", validationErrors));
                    return Results.BadRequest(new
                    {
                        Error = "Validation failed",
                        Message = "The request contains invalid data for structured fields.",
                        Details = validationErrors
                    });
                }

                var sanitizedRoster = sanitizer.SanitizeModel(roster, logger);
                var player = new PlayerUpsertDto
                {
                    SportCode = "MLB",
                    TeamCode = sanitizedRoster.TeamCode?.Trim() ?? sanitizedRoster.TeamName?.Trim() ?? string.Empty,
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

                var hasStats = !string.IsNullOrWhiteSpace(sanitizedRoster.Bats)
                    || !string.IsNullOrWhiteSpace(sanitizedRoster.Throws)
                    || sanitizedRoster.BattingAverage.HasValue
                    || sanitizedRoster.HomeRuns.HasValue
                    || sanitizedRoster.Era.HasValue;

                var createdId = hasStats
                    ? await writeService.CreatePlayerWithStats(logger, player, new PlayerStatsUpsertDto
                    {
                        Bats = sanitizedRoster.Bats,
                        Throws = sanitizedRoster.Throws,
                        BattingAverage = sanitizedRoster.BattingAverage,
                        HomeRuns = sanitizedRoster.HomeRuns,
                        Era = sanitizedRoster.Era
                    })
                    : await writeService.CreatePlayer(logger, player);

                if (createdId == null)
                {
                    return Results.Problem("Error adding to MLB Roster, check the logs.");
                }

                return Results.Ok("Added to MLB Roster.");
            });

            mlb.MapPut("roster", async (ILogger<MLBRoster> logger, IPlayersV2WriteService writeService, IXssValidationService xssValidator, IInputSanitizationService sanitizer, IInputValidationService validator, MLBRoster roster) =>
            {
                var (isValid, violations) = xssValidator.ValidateTextFields(roster, logger);
                if (!isValid)
                {
                    logger.LogWarning("XSS attempt blocked in MLB roster update. Violations: {Violations}", string.Join(", ", violations));
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
                    logger.LogWarning("Validation errors in MLB roster update. Errors: {Errors}", string.Join(", ", validationErrors));
                    return Results.BadRequest(new
                    {
                        Error = "Validation failed",
                        Message = "The request contains invalid data for structured fields.",
                        Details = validationErrors
                    });
                }

                var sanitizedRoster = sanitizer.SanitizeModel(roster, logger);
                if (!int.TryParse(sanitizedRoster.PlayerId, out var playerId) || playerId <= 0)
                {
                    return Results.BadRequest(new
                    {
                        Error = "Validation failed",
                        Message = "A valid numeric playerID is required for MLB roster update."
                    });
                }

                var player = new PlayerUpsertDto
                {
                    SportCode = "MLB",
                    TeamCode = sanitizedRoster.TeamCode?.Trim() ?? sanitizedRoster.TeamName?.Trim() ?? string.Empty,
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

                var hasStats = !string.IsNullOrWhiteSpace(sanitizedRoster.Bats)
                    || !string.IsNullOrWhiteSpace(sanitizedRoster.Throws)
                    || sanitizedRoster.BattingAverage.HasValue
                    || sanitizedRoster.HomeRuns.HasValue
                    || sanitizedRoster.Era.HasValue;

                var updateOutcome = hasStats
                    ? await writeService.UpdatePlayerWithStatsDetailed(logger, playerId, player, new PlayerStatsUpsertDto
                    {
                        Bats = sanitizedRoster.Bats,
                        Throws = sanitizedRoster.Throws,
                        BattingAverage = sanitizedRoster.BattingAverage,
                        HomeRuns = sanitizedRoster.HomeRuns,
                        Era = sanitizedRoster.Era
                    })
                    : await writeService.UpdatePlayerDetailed(logger, playerId, player);

                if (updateOutcome == PlayerWriteOutcome.NotFound)
                {
                    return Results.NotFound("MLB player was not found or update failed.");
                }

                if (updateOutcome == PlayerWriteOutcome.InvalidTeam)
                {
                    return Results.BadRequest(new
                    {
                        Error = "Validation failed",
                        Message = "Team code could not be resolved for MLB."
                    });
                }

                if (updateOutcome == PlayerWriteOutcome.StatsFailed)
                {
                    return Results.Problem("MLB player update failed while writing stats. Check logs for details.");
                }

                if (updateOutcome == PlayerWriteOutcome.Error)
                {
                    return Results.Problem("Error updating MLB Roster, check the logs.");
                }

                return Results.Ok("Updated MLB Roster.");
            });

            mlb.MapDelete("roster", async (ILogger<MLBRoster> logger, IPlayersV2WriteService writeService, string playerId) =>
            {
                if (!int.TryParse(playerId, out var parsedPlayerId) || parsedPlayerId <= 0)
                {
                    return Results.BadRequest(new
                    {
                        Error = "Validation failed",
                        Message = "A valid numeric playerId query parameter is required for MLB delete."
                    });
                }

                var deleted = await writeService.DeletePlayer(logger, parsedPlayerId);
                if (deleted)
                {
                    return Results.Ok("Deleted from MLB Roster.");
                }

                return Results.NotFound("MLB player was not found or delete failed.");
            });
        }

        MapMlbRoutes(routes.MapGroup("api/mlb"));
        MapMlbRoutes(routes.MapGroup("api/v2/mlb"));
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