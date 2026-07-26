using AgilitySportsAPI.Data;
using AgilitySportsAPI.Dtos;
using Microsoft.Data.SqlClient;

namespace AgilitySportsAPI.Services;

public enum PlayerWriteOutcome
{
    Success,
    NotFound,
    InvalidTeam,
    StatsFailed,
    Error
}

public interface IPlayersV2WriteService
{
    Task<int?> CreatePlayer(ILogger logger, PlayerUpsertDto player);
    Task<int?> CreatePlayerWithStats(ILogger logger, PlayerUpsertDto player, PlayerStatsUpsertDto stats);
    Task<bool> UpdatePlayer(ILogger logger, int playerId, PlayerUpsertDto player);
    Task<bool> UpdatePlayerWithStats(ILogger logger, int playerId, PlayerUpsertDto player, PlayerStatsUpsertDto stats);
    Task<PlayerWriteOutcome> UpdatePlayerDetailed(ILogger logger, int playerId, PlayerUpsertDto player);
    Task<PlayerWriteOutcome> UpdatePlayerWithStatsDetailed(ILogger logger, int playerId, PlayerUpsertDto player, PlayerStatsUpsertDto stats);
    Task<bool> DeletePlayer(ILogger logger, int playerId);
    Task<bool> UpsertPlayerStats(ILogger logger, int playerId, PlayerStatsUpsertDto stats);
}

public class PlayersV2WriteService : BaseRepo, IPlayersV2WriteService
{
    private readonly IPlayersRepo _playersRepo;

    public PlayersV2WriteService(IConfiguration configuration, IPlayersRepo playersRepo) : base(configuration)
    {
        _playersRepo = playersRepo;
    }

    private async Task<SqlConnection> OpenConnectionAsync()
    {
        var connection = new SqlConnection(base.connectionString);
        await base.GenToken(connection);
        await connection.OpenAsync();
        return connection;
    }

    public async Task<int?> CreatePlayer(ILogger logger, PlayerUpsertDto player)
    {
        using var connection = await OpenConnectionAsync();

        using var transaction = connection.BeginTransaction();
        try
        {
            var teamCode = NormalizeTeamCode(player.TeamCode);
            if (string.IsNullOrWhiteSpace(teamCode) || teamCode.Length != 3)
            {
                logger.LogError("Invalid team code '{Team}' for sport '{SportCode}'. Expected 3-letter TeamCode.", player.TeamCode, player.SportCode);
                transaction.Rollback();
                return null;
            }

            player.TeamCode = teamCode;

            var createdId = await _playersRepo.CreatePlayer(connection, transaction, logger, player);
            transaction.Commit();
            return createdId;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating V2 player.");
            transaction.Rollback();
            return null;
        }
    }

    public async Task<int?> CreatePlayerWithStats(ILogger logger, PlayerUpsertDto player, PlayerStatsUpsertDto stats)
    {
        using var connection = await OpenConnectionAsync();

        using var transaction = connection.BeginTransaction();
        try
        {
            var teamCode = NormalizeTeamCode(player.TeamCode);
            if (string.IsNullOrWhiteSpace(teamCode) || teamCode.Length != 3)
            {
                logger.LogError("Invalid team code '{Team}' for sport '{SportCode}'. Expected 3-letter TeamCode.", player.TeamCode, player.SportCode);
                transaction.Rollback();
                return null;
            }

            player.TeamCode = teamCode;

            var createdId = await _playersRepo.CreatePlayer(connection, transaction, logger, player);
            if (createdId <= 0)
            {
                transaction.Rollback();
                return null;
            }

            var saved = await _playersRepo.UpsertPlayerStats(connection, transaction, logger, createdId, player.SportCode, stats);
            if (!saved)
            {
                transaction.Rollback();
                return null;
            }

            transaction.Commit();
            return createdId;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating V2 player with stats.");
            transaction.Rollback();
            return null;
        }
    }

    public async Task<bool> UpdatePlayer(ILogger logger, int playerId, PlayerUpsertDto player)
    {
        var outcome = await UpdatePlayerDetailed(logger, playerId, player);
        return outcome == PlayerWriteOutcome.Success;
    }

    public async Task<PlayerWriteOutcome> UpdatePlayerDetailed(ILogger logger, int playerId, PlayerUpsertDto player)
    {
        using var connection = await OpenConnectionAsync();

        using var transaction = connection.BeginTransaction();
        try
        {
            var teamCode = NormalizeTeamCode(player.TeamCode);
            if (string.IsNullOrWhiteSpace(teamCode) || teamCode.Length != 3)
            {
                logger.LogError("Invalid team code '{Team}' for sport '{SportCode}'. Expected 3-letter TeamCode.", player.TeamCode, player.SportCode);
                transaction.Rollback();
                return PlayerWriteOutcome.InvalidTeam;
            }

            player.TeamCode = teamCode;

            var updated = await _playersRepo.UpdatePlayer(connection, transaction, logger, playerId, player);
            if (!updated)
            {
                transaction.Rollback();
                return PlayerWriteOutcome.NotFound;
            }

            transaction.Commit();
            return PlayerWriteOutcome.Success;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating V2 player {PlayerId}.", playerId);
            transaction.Rollback();
            return PlayerWriteOutcome.Error;
        }
    }

    public async Task<bool> UpdatePlayerWithStats(ILogger logger, int playerId, PlayerUpsertDto player, PlayerStatsUpsertDto stats)
    {
        var outcome = await UpdatePlayerWithStatsDetailed(logger, playerId, player, stats);
        return outcome == PlayerWriteOutcome.Success;
    }

    public async Task<PlayerWriteOutcome> UpdatePlayerWithStatsDetailed(ILogger logger, int playerId, PlayerUpsertDto player, PlayerStatsUpsertDto stats)
    {
        using var connection = await OpenConnectionAsync();

        using var transaction = connection.BeginTransaction();
        try
        {
            var teamCode = NormalizeTeamCode(player.TeamCode);
            if (string.IsNullOrWhiteSpace(teamCode) || teamCode.Length != 3)
            {
                logger.LogError("Invalid team code '{Team}' for sport '{SportCode}'. Expected 3-letter TeamCode.", player.TeamCode, player.SportCode);
                transaction.Rollback();
                return PlayerWriteOutcome.InvalidTeam;
            }

            player.TeamCode = teamCode;

            var updated = await _playersRepo.UpdatePlayer(connection, transaction, logger, playerId, player);
            if (!updated)
            {
                transaction.Rollback();
                return PlayerWriteOutcome.NotFound;
            }

            var saved = await _playersRepo.UpsertPlayerStats(connection, transaction, logger, playerId, player.SportCode, stats);
            if (!saved)
            {
                transaction.Rollback();
                return PlayerWriteOutcome.StatsFailed;
            }

            transaction.Commit();
            return PlayerWriteOutcome.Success;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating V2 player {PlayerId} with stats.", playerId);
            transaction.Rollback();
            return PlayerWriteOutcome.Error;
        }
    }

    public async Task<bool> DeletePlayer(ILogger logger, int playerId)
    {
        using var connection = await OpenConnectionAsync();

        using var transaction = connection.BeginTransaction();
        try
        {
            var sportCode = await _playersRepo.GetPlayerSportCode(connection, transaction, playerId);
            if (string.IsNullOrWhiteSpace(sportCode))
            {
                transaction.Rollback();
                return false;
            }

            await _playersRepo.DeletePlayerStats(connection, transaction, logger, playerId, sportCode);
            var deleted = await _playersRepo.DeletePlayer(connection, transaction, logger, playerId);
            if (!deleted)
            {
                transaction.Rollback();
                return false;
            }

            transaction.Commit();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting V2 player {PlayerId}.", playerId);
            transaction.Rollback();
            return false;
        }
    }

    public async Task<bool> UpsertPlayerStats(ILogger logger, int playerId, PlayerStatsUpsertDto stats)
    {
        using var connection = await OpenConnectionAsync();

        using var transaction = connection.BeginTransaction();
        try
        {
            var sportCode = await _playersRepo.GetPlayerSportCode(connection, transaction, playerId);
            if (string.IsNullOrWhiteSpace(sportCode))
            {
                transaction.Rollback();
                return false;
            }

            var saved = await _playersRepo.UpsertPlayerStats(connection, transaction, logger, playerId, sportCode, stats);
            if (!saved)
            {
                transaction.Rollback();
                return false;
            }

            transaction.Commit();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting V2 player stats for PlayerId {PlayerId}.", playerId);
            transaction.Rollback();
            return false;
        }
    }

    private static string? NormalizeTeamCode(string? teamCode)
    {
        if (string.IsNullOrWhiteSpace(teamCode))
        {
            return null;
        }

        return teamCode.Trim().ToUpperInvariant();
    }
}
