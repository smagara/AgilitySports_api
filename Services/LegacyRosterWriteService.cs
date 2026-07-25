using System.Configuration;
using AgilitySportsAPI.Data;
using AgilitySportsAPI.Dtos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AgilitySportsAPI.Services;

public class LegacyPlayerWriteInput
{
    public string SportCode { get; set; } = null!;
    public string Team { get; set; } = null!;
    public string? League { get; set; }
    public string? Position { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Height { get; set; }
    public int? Weight { get; set; }
    public int? Number { get; set; }
    public string? College { get; set; }
    public string? BirthPlace { get; set; }
    public short? DraftYear { get; set; }
    public short? SeasonYear { get; set; }
}

public class LegacyWriteResult
{
    public bool Succeeded { get; init; }
    public bool NotFound { get; init; }
    public int? PlayerId { get; init; }
    public string? Error { get; init; }

    public static LegacyWriteResult Success(int? playerId = null) => new() { Succeeded = true, PlayerId = playerId };
    public static LegacyWriteResult Missing(string error) => new() { Succeeded = false, Error = error };
    public static LegacyWriteResult MissingPlayer(string error) => new() { Succeeded = false, NotFound = true, Error = error };
}

public interface ILegacyRosterWriteService
{
    Task<LegacyWriteResult> CreateAsync(ILogger logger, LegacyPlayerWriteInput input, PlayerStatsUpsertDto? stats);
    Task<LegacyWriteResult> UpdateAsync(ILogger logger, int playerId, LegacyPlayerWriteInput input, PlayerStatsUpsertDto? stats);
    Task<LegacyWriteResult> DeleteAsync(ILogger logger, int playerId);
}

public class LegacyRosterWriteService : BaseRepo, ILegacyRosterWriteService
{
    private readonly IPlayersV2WriteService _playersV2WriteService;

    public LegacyRosterWriteService(IConfiguration configuration, IPlayersV2WriteService playersV2WriteService)
        : base(configuration)
    {
        _playersV2WriteService = playersV2WriteService;
    }

    public async Task<LegacyWriteResult> CreateAsync(ILogger logger, LegacyPlayerWriteInput input, PlayerStatsUpsertDto? stats)
    {
        try
        {
            var mapped = await BuildUpsertDtoAsync(input);
            if (!mapped.Succeeded)
            {
                return LegacyWriteResult.Missing(mapped.Error ?? "Unable to map legacy create request to V2 player model.");
            }

            var createdId = await _playersV2WriteService.CreatePlayer(logger, mapped.Player!);
            if (createdId == null)
            {
                return LegacyWriteResult.Missing("Unable to create player in DB V2.");
            }

            var statsSave = await SaveStatsIfPresent(logger, createdId.Value, input.SportCode, stats);
            if (!statsSave.Succeeded)
            {
                return statsSave;
            }

            return LegacyWriteResult.Success(createdId.Value);
        }
        catch (Exception ex)
        {
            logger.LogError("Legacy create bridge failed: {Message}", ex.Message);
            return LegacyWriteResult.Missing("Unable to create player due to an internal error.");
        }
    }

    public async Task<LegacyWriteResult> UpdateAsync(ILogger logger, int playerId, LegacyPlayerWriteInput input, PlayerStatsUpsertDto? stats)
    {
        try
        {
            var mapped = await BuildUpsertDtoAsync(input);
            if (!mapped.Succeeded)
            {
                return LegacyWriteResult.Missing(mapped.Error ?? "Unable to map legacy update request to V2 player model.");
            }

            var updated = await _playersV2WriteService.UpdatePlayer(logger, playerId, mapped.Player!);
            if (!updated)
            {
                return LegacyWriteResult.MissingPlayer($"PlayerId {playerId} was not found or update failed.");
            }

            var statsSave = await SaveStatsIfPresent(logger, playerId, input.SportCode, stats);
            if (!statsSave.Succeeded)
            {
                return statsSave;
            }

            return LegacyWriteResult.Success(playerId);
        }
        catch (Exception ex)
        {
            logger.LogError("Legacy update bridge failed for PlayerId {PlayerId}: {Message}", playerId, ex.Message);
            return LegacyWriteResult.Missing("Unable to update player due to an internal error.");
        }
    }

    public async Task<LegacyWriteResult> DeleteAsync(ILogger logger, int playerId)
    {
        try
        {
            var deleted = await _playersV2WriteService.DeletePlayer(logger, playerId);
            if (!deleted)
            {
                return LegacyWriteResult.MissingPlayer($"PlayerId {playerId} was not found or delete failed.");
            }

            return LegacyWriteResult.Success(playerId);
        }
        catch (Exception ex)
        {
            logger.LogError("Legacy delete bridge failed for PlayerId {PlayerId}: {Message}", playerId, ex.Message);
            return LegacyWriteResult.Missing("Unable to delete player due to an internal error.");
        }
    }

    private async Task<(bool Succeeded, string? Error, PlayerUpsertDto? Player)> BuildUpsertDtoAsync(LegacyPlayerWriteInput input)
    {
        var sportCode = (input.SportCode ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(sportCode))
        {
            return (false, "Sport code is required.", null);
        }

        var teamCode = await ResolveTeamCodeAsync(sportCode, input.Team);
        if (string.IsNullOrWhiteSpace(teamCode))
        {
            return (false, $"Unknown team '{input.Team}' for sport '{sportCode}'.", null);
        }

        var positionCode = await ResolvePositionCodeAsync(sportCode, input.Position);
        if (!string.IsNullOrWhiteSpace(input.Position) && string.IsNullOrWhiteSpace(positionCode))
        {
            return (false, $"Unknown position '{input.Position}' for sport '{sportCode}'.", null);
        }

        return (true, null, new PlayerUpsertDto
        {
            SportCode = sportCode,
            TeamCode = teamCode,
            PositionCode = positionCode,
            FirstName = input.FirstName,
            LastName = input.LastName,
            DateOfBirth = input.DateOfBirth,
            Height = NormalizeNullable(input.Height),
            Weight = input.Weight,
            Number = input.Number,
            College = NormalizeNullable(input.College),
            Birthplace = NormalizeNullable(input.BirthPlace),
            DraftYear = input.DraftYear,
            SeasonYear = input.SeasonYear
        });
    }

    private async Task<LegacyWriteResult> SaveStatsIfPresent(ILogger logger, int playerId, string sportCode, PlayerStatsUpsertDto? stats)
    {
        if (stats == null || !HasAnyStats(sportCode, stats))
        {
            return LegacyWriteResult.Success(playerId);
        }

        var saved = await _playersV2WriteService.UpsertPlayerStats(logger, playerId, stats);
        if (!saved)
        {
            return LegacyWriteResult.MissingPlayer($"PlayerId {playerId} stats upsert failed.");
        }

        return LegacyWriteResult.Success(playerId);
    }

    private bool HasAnyStats(string sportCode, PlayerStatsUpsertDto stats)
    {
        var normalizedSport = (sportCode ?? string.Empty).Trim().ToUpperInvariant();
        return normalizedSport switch
        {
            "MLB" => !string.IsNullOrWhiteSpace(stats.Bats)
                || !string.IsNullOrWhiteSpace(stats.Throws)
                || stats.BattingAverage.HasValue
                || stats.HomeRuns.HasValue
                || stats.Era.HasValue,
            "NBA" => stats.PointsPerGame.HasValue
                || stats.ReboundsPerGame.HasValue
                || stats.AssistsPerGame.HasValue,
            "NFL" => stats.Sacks.HasValue
                || stats.Touchdowns.HasValue,
            "NHL" => !string.IsNullOrWhiteSpace(stats.Handed)
                || stats.Goals.HasValue
                || stats.PenaltyMinutes.HasValue
                || stats.Points.HasValue
                || stats.SavePct.HasValue,
            _ => false
        };
    }

    private async Task<string?> ResolveTeamCodeAsync(string sportCode, string? teamOrCode)
    {
        if (string.IsNullOrWhiteSpace(teamOrCode))
        {
            return null;
        }

        var value = teamOrCode.Trim();
        using var connection = new SqlConnection(connectionString);
        await GenToken(connection);

        var sql = @"
            select top 1 teamCode
            from core.Teams
            where sportCode = @sportCode
              and (
                    upper(teamCode) = upper(@value)
                      or upper(teamShortName) = upper(@value)
                 or upper(teamName) = upper(@value)
              );";

        return await connection.QuerySingleOrDefaultAsync<string>(sql, new { sportCode, value });
    }

    private async Task<string?> ResolvePositionCodeAsync(string sportCode, string? positionOrCode)
    {
        if (string.IsNullOrWhiteSpace(positionOrCode))
        {
            return null;
        }

        var value = positionOrCode.Trim();
        using var connection = new SqlConnection(connectionString);
        await GenToken(connection);

        var sql = @"
            select top 1 positionCode
            from reference.PositionCodes
            where sportCode = @sportCode
              and (
                    upper(positionCode) = upper(@value)
                 or upper(positionDesc) = upper(@value)
              );";

        return await connection.QuerySingleOrDefaultAsync<string>(sql, new { sportCode, value });
    }

    private static string? NormalizeNullable(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public static int? ParseNullableInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return int.TryParse(value.Trim(), out var parsed) ? parsed : null;
    }

    public static short? ParseNullableShort(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return short.TryParse(value.Trim(), out var parsed) ? parsed : null;
    }

    public static (string? FirstName, string? LastName) SplitName(string? fullName)
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
}
