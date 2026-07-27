// This file contains the endpoints for NHL-related operations such as CRUD operations on the NHL roster.

using AgilitySportsAPI.Data;
using AgilitySportsAPI.Dtos;
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
        void MapNhlRoutes(RouteGroupBuilder nhl)
        {
            // Read
            nhl.MapGet("roster", async (ILogger<NHLRoster> logger, int? playerId, INHLRepo repo) =>
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
            nhl.MapPost("roster", async (ILogger<NHLRoster> logger, IPlayersV2WriteService writeService, IXssValidationService xssValidator, IInputSanitizationService sanitizer, IInputValidationService validator, NHLRoster roster) =>
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

                // Validate structured fields (height, weight, age, position, handed, etc.)
                var (isValidStructured, validationErrors) = validator.ValidateModel(roster, logger);

                if (!isValidStructured)
                { 
                    logger.LogWarning("Validation errors in NHL roster creation. Errors: {Errors}", string.Join(", ", validationErrors));
                    return Results.BadRequest(new
                    {
                        Error = "Validation failed",
                        Message = "The request contains invalid data for structured fields.",
                        Details = validationErrors
                    });
                }

                // Sanitize input after all validation passes
                var sanitizedRoster = sanitizer.SanitizeModel(roster, logger);

                var statValidationErrors = ValidateNhlStats(sanitizedRoster);
                if (statValidationErrors.Count > 0)
                {
                    return Results.BadRequest(new
                    {
                        Error = "Validation failed",
                        Message = "The request contains invalid NHL stats values.",
                        Details = statValidationErrors
                    });
                }

                var (nameFirst, nameLast) = SplitName(sanitizedRoster.Name);
                var player = new PlayerUpsertDto
                {
                    SportCode = "NHL",
                    TeamCode = sanitizedRoster.TeamCode?.Trim() ?? sanitizedRoster.Team?.Trim() ?? string.Empty,
                    PositionCode = sanitizedRoster.Position?.Trim(),
                    FirstName = sanitizedRoster.FirstName ?? nameFirst,
                    LastName = sanitizedRoster.LastName ?? nameLast,
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

                var hasStats = !string.IsNullOrWhiteSpace(sanitizedRoster.Handed)
                    || sanitizedRoster.Goals.HasValue
                    || sanitizedRoster.PenaltyMinutes.HasValue
                    || sanitizedRoster.Points.HasValue
                    || sanitizedRoster.SavePct.HasValue;

                var createdId = hasStats
                    ? await writeService.CreatePlayerWithStats(logger, player, new PlayerStatsUpsertDto
                    {
                        Handed = sanitizedRoster.Handed,
                        Goals = sanitizedRoster.Goals,
                        PenaltyMinutes = sanitizedRoster.PenaltyMinutes,
                        Points = sanitizedRoster.Points,
                        SavePct = sanitizedRoster.SavePct
                    })
                    : await writeService.CreatePlayer(logger, player);

                if (createdId != null)
                { 
                    return Results.Ok("Added to NHL Roster.");
                }
                else
                { 
                    return Results.Problem("Error adding to NHL Roster, check the logs.");
                }
            });

            // Update
            nhl.MapPut("roster", async (ILogger<NHLRoster> logger, IPlayersV2WriteService writeService, IXssValidationService xssValidator, IInputSanitizationService sanitizer, IInputValidationService validator, NHLRoster roster) =>
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

                // Validate structured fields (height, weight, age, position, etc.)
                var (isValidStructured, validationErrors) = validator.ValidateModel(roster, logger);

                if (!isValidStructured)
                {
                    logger.LogWarning("Validation errors in NHL roster update. Errors: {Errors}", string.Join(", ", validationErrors));
                    return Results.BadRequest(new
                    {
                        Error = "Validation failed",
                        Message = "The request contains invalid data for structured fields.",
                        Details = validationErrors
                    });
                }

                // Sanitize input after all validation passes
                var sanitizedRoster = sanitizer.SanitizeModel(roster, logger);

                var statValidationErrors = ValidateNhlStats(sanitizedRoster);
                if (statValidationErrors.Count > 0)
                {
                    return Results.BadRequest(new
                    {
                        Error = "Validation failed",
                        Message = "The request contains invalid NHL stats values.",
                        Details = statValidationErrors
                    });
                }

                var (nameFirst, nameLast) = SplitName(sanitizedRoster.Name);
                var player = new PlayerUpsertDto
                {
                    SportCode = "NHL",
                    TeamCode = sanitizedRoster.TeamCode?.Trim() ?? sanitizedRoster.Team?.Trim() ?? string.Empty,
                    PositionCode = sanitizedRoster.Position?.Trim(),
                    FirstName = sanitizedRoster.FirstName ?? nameFirst,
                    LastName = sanitizedRoster.LastName ?? nameLast,
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

                var hasStats = !string.IsNullOrWhiteSpace(sanitizedRoster.Handed)
                    || sanitizedRoster.Goals.HasValue
                    || sanitizedRoster.PenaltyMinutes.HasValue
                    || sanitizedRoster.Points.HasValue
                    || sanitizedRoster.SavePct.HasValue;

                var updateOutcome = hasStats
                    ? await writeService.UpdatePlayerWithStatsDetailed(logger, sanitizedRoster.PlayerId, player, new PlayerStatsUpsertDto
                    {
                        Handed = sanitizedRoster.Handed,
                        Goals = sanitizedRoster.Goals,
                        PenaltyMinutes = sanitizedRoster.PenaltyMinutes,
                        Points = sanitizedRoster.Points,
                        SavePct = sanitizedRoster.SavePct
                    })
                    : await writeService.UpdatePlayerDetailed(logger, sanitizedRoster.PlayerId, player);

                if (updateOutcome == PlayerWriteOutcome.Success)
                {
                    return Results.Ok("Updated the NHL Roster.");
                }

                if (updateOutcome == PlayerWriteOutcome.NotFound)
                {
                    return Results.NotFound("NHL player was not found or update failed.");
                }

                if (updateOutcome == PlayerWriteOutcome.InvalidTeam)
                {
                    return Results.BadRequest(new
                    {
                        Error = "Validation failed",
                        Message = "Team code could not be resolved for NHL."
                    });
                }

                if (updateOutcome == PlayerWriteOutcome.StatsFailed)
                {
                    return Results.Problem("NHL player update failed while writing stats. Check logs for details.");
                }

                return Results.Problem("Error updating NHL Roster, check the logs.");
            });

            // Delete
            nhl.MapDelete("roster", async (ILogger<NHLRoster> logger, IPlayersV2WriteService writeService, int playerId) =>
            {
                var deleted = await writeService.DeletePlayer(logger, playerId);

                if (deleted)
                {
                    return Results.Ok("Deleted from NHL Roster.");
                }
                else
                {
                    return Results.NotFound("NHL player was not found or delete failed.");
                }
            });
        }

        MapNhlRoutes(routes.MapGroup("api/v2/nhl"));
    }

    private static int? ParseNullableInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return int.TryParse(value.Trim(), out var parsed) ? parsed : null;
    }

    private static (string? FirstName, string? LastName) SplitName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return (null, null);
        }

        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            return (parts[0], null);
        }

        return (parts[0], string.Join(' ', parts.Skip(1)));
    }

    private static List<string> ValidateNhlStats(NHLRoster roster)
    {
        var errors = new List<string>();

        if (roster.Goals.HasValue && roster.Goals.Value < 0)
        {
            errors.Add("Goals must be greater than or equal to 0.");
        }

        if (roster.PenaltyMinutes.HasValue && roster.PenaltyMinutes.Value < 0)
        {
            errors.Add("PenaltyMinutes must be greater than or equal to 0.");
        }

        if (roster.Points.HasValue && roster.Points.Value < 0)
        {
            errors.Add("Points must be greater than or equal to 0.");
        }

        if (roster.SavePct.HasValue && (roster.SavePct.Value < 0m || roster.SavePct.Value > 1m))
        {
            errors.Add("SavePct must be between 0 and 1 (example: 0.915).");
        }

        return errors;
    }
}