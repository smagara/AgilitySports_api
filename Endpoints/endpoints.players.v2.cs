using AgilitySportsAPI.Data;
using AgilitySportsAPI.Dtos;
using AgilitySportsAPI.Services;

public static class PlayersV2Endpoints
{
    public static void MapPlayersV2Endpoints(this IEndpointRouteBuilder routes)
    {
        var players = routes.MapGroup("api/v2/players");

        players.MapGet("", async (
            ILogger<PlayerSummaryDto> logger,
            IPlayersRepo repo,
            string? sportCode,
            string? teamCode,
            string? positionCode,
            string? search) =>
        {
            var results = await repo.GetPlayers(logger, sportCode, teamCode, positionCode, search);
            if (results != null)
            {
                return Results.Ok(results);
            }

            return Results.Problem("Error fetching V2 Players, ask your admin to check the logs.");
        });

        players.MapGet("{playerId:int}", async (
            ILogger<PlayerSummaryDto> logger,
            IPlayersRepo repo,
            int playerId) =>
        {
            var player = await repo.GetPlayerById(logger, playerId);
            if (player != null)
            {
                return Results.Ok(player);
            }

            return Results.NotFound($"V2 PlayerId {playerId} was not found.");
        });

        players.MapPost("", async (
            ILogger<PlayerSummaryDto> logger,
            IPlayersV2WriteService writeService,
            IXssValidationService xssValidator,
            IInputSanitizationService sanitizer,
            PlayerUpsertDto player) =>
        {
            var (isValid, violations) = xssValidator.ValidateTextFields(player, logger);
            if (!isValid)
            {
                logger.LogWarning("XSS attempt blocked in V2 player creation. Violations: {Violations}", string.Join(", ", violations));
                return Results.BadRequest(new
                {
                    Error = "XSS attempt detected",
                    Message = "The request contains potentially malicious content and has been blocked for security reasons.",
                    Details = violations
                });
            }

            var createValidationErrors = ValidatePlayerWriteRequest(player);
            if (createValidationErrors.Count > 0)
            {
                return Results.BadRequest(new
                {
                    Error = "Validation failed",
                    Message = "The request is missing required fields for v2 player writes.",
                    Details = createValidationErrors
                });
            }

            var sanitizedPlayer = sanitizer.SanitizeModel(player, logger);
            var createdId = await writeService.CreatePlayer(logger, sanitizedPlayer);

            if (createdId == null)
            {
                return Results.Problem("Error creating V2 Player, ask your admin to check the logs.");
            }

            return Results.Created($"/api/v2/players/{createdId}", new { playerId = createdId });
        });

        players.MapPut("{playerId:int}", async (
            ILogger<PlayerSummaryDto> logger,
            IPlayersV2WriteService writeService,
            IXssValidationService xssValidator,
            IInputSanitizationService sanitizer,
            int playerId,
            PlayerUpsertDto player) =>
        {
            var (isValid, violations) = xssValidator.ValidateTextFields(player, logger);
            if (!isValid)
            {
                logger.LogWarning("XSS attempt blocked in V2 player update. Violations: {Violations}", string.Join(", ", violations));
                return Results.BadRequest(new
                {
                    Error = "XSS attempt detected",
                    Message = "The request contains potentially malicious content and has been blocked for security reasons.",
                    Details = violations
                });
            }

            var updateValidationErrors = ValidatePlayerWriteRequest(player);
            if (updateValidationErrors.Count > 0)
            {
                return Results.BadRequest(new
                {
                    Error = "Validation failed",
                    Message = "The request is missing required fields for v2 player writes.",
                    Details = updateValidationErrors
                });
            }

            var sanitizedPlayer = sanitizer.SanitizeModel(player, logger);
            var updated = await writeService.UpdatePlayer(logger, playerId, sanitizedPlayer);
            if (!updated)
            {
                return Results.NotFound($"V2 PlayerId {playerId} was not found or update failed.");
            }

            return Results.Ok($"Updated V2 PlayerId {playerId}.");
        });

        players.MapDelete("{playerId:int}", async (
            ILogger<PlayerSummaryDto> logger,
            IPlayersV2WriteService writeService,
            int playerId) =>
        {
            var deleted = await writeService.DeletePlayer(logger, playerId);
            if (!deleted)
            {
                return Results.NotFound($"V2 PlayerId {playerId} was not found or delete failed.");
            }

            return Results.Ok($"Deleted V2 PlayerId {playerId}.");
        });

        players.MapGet("{playerId:int}/stats", async (
            ILogger<PlayerSummaryDto> logger,
            IPlayersRepo repo,
            int playerId) =>
        {
            var stats = await repo.GetPlayerStats(logger, playerId);
            if (stats != null)
            {
                return Results.Ok(stats);
            }

            return Results.NotFound($"V2 PlayerId {playerId} stats were not found.");
        });

        players.MapPut("{playerId:int}/stats", async (
            ILogger<PlayerSummaryDto> logger,
            IPlayersV2WriteService writeService,
            IXssValidationService xssValidator,
            IInputSanitizationService sanitizer,
            int playerId,
            PlayerStatsUpsertDto stats) =>
        {
            var (isValid, violations) = xssValidator.ValidateTextFields(stats, logger);
            if (!isValid)
            {
                logger.LogWarning("XSS attempt blocked in V2 player stats upsert. Violations: {Violations}", string.Join(", ", violations));
                return Results.BadRequest(new
                {
                    Error = "XSS attempt detected",
                    Message = "The request contains potentially malicious content and has been blocked for security reasons.",
                    Details = violations
                });
            }

            var sanitizedStats = sanitizer.SanitizeModel(stats, logger);
            var updated = await writeService.UpsertPlayerStats(logger, playerId, sanitizedStats);
            if (!updated)
            {
                return Results.NotFound($"V2 PlayerId {playerId} was not found or stats upsert failed.");
            }

            return Results.Ok($"Upserted V2 stats for PlayerId {playerId}.");
        });
    }

    private static List<string> ValidatePlayerWriteRequest(PlayerUpsertDto player)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(player.SportCode))
        {
            errors.Add("SportCode is required.");
        }

        if (string.IsNullOrWhiteSpace(player.TeamCode))
        {
            errors.Add("TeamCode is required.");
        }

        return errors;
    }
}
