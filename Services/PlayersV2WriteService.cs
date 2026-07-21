using AgilitySportsAPI.Data;
using AgilitySportsAPI.Dtos;
using Microsoft.Data.SqlClient;
using System.Configuration;

namespace AgilitySportsAPI.Services;

public interface IPlayersV2WriteService
{
    Task<int?> CreatePlayer(ILogger logger, PlayerUpsertDto player);
    Task<bool> UpdatePlayer(ILogger logger, int playerId, PlayerUpsertDto player);
    Task<bool> DeletePlayer(ILogger logger, int playerId);
    Task<bool> UpsertPlayerStats(ILogger logger, int playerId, PlayerStatsUpsertDto stats);
}

public class PlayersV2WriteService : IPlayersV2WriteService
{
    private readonly string _connectionString;
    private readonly IPlayersRepo _playersRepo;

    public PlayersV2WriteService(IConfiguration configuration, IPlayersRepo playersRepo)
    {
        _playersRepo = playersRepo;
        _connectionString = configuration.GetConnectionString("DockerConnectionV2") ?? "";
        if (_connectionString == "")
        {
            throw new ConfigurationErrorsException("ConnectionStrings:DockerConnectionV2 must be set for V2 player APIs.");
        }
    }

    public async Task<int?> CreatePlayer(ILogger logger, PlayerUpsertDto player)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();
        try
        {
            var createdId = await _playersRepo.CreatePlayer(connection, transaction, logger, player);
            transaction.Commit();
            return createdId;
        }
        catch (Exception ex)
        {
            logger.LogError("Error creating V2 player: " + ex.Message);
            transaction.Rollback();
            return null;
        }
    }

    public async Task<bool> UpdatePlayer(ILogger logger, int playerId, PlayerUpsertDto player)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();
        try
        {
            var updated = await _playersRepo.UpdatePlayer(connection, transaction, logger, playerId, player);
            if (!updated)
            {
                transaction.Rollback();
                return false;
            }

            transaction.Commit();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Error updating V2 player {PlayerId}: " + ex.Message, playerId);
            transaction.Rollback();
            return false;
        }
    }

    public async Task<bool> DeletePlayer(ILogger logger, int playerId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

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
            logger.LogError("Error deleting V2 player {PlayerId}: " + ex.Message, playerId);
            transaction.Rollback();
            return false;
        }
    }

    public async Task<bool> UpsertPlayerStats(ILogger logger, int playerId, PlayerStatsUpsertDto stats)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

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
            logger.LogError("Error upserting V2 player stats for PlayerId {PlayerId}: " + ex.Message, playerId);
            transaction.Rollback();
            return false;
        }
    }
}
