using AgilitySportsAPI.Dtos;
using Microsoft.Data.SqlClient;

namespace AgilitySportsAPI.Data;

public interface IPlayersRepo
{
    Task<IEnumerable<PlayerSummaryDto>?> GetPlayers(
        ILogger<PlayerSummaryDto> logger,
        string? sportCode,
        string? teamCode,
        string? positionCode,
        string? search);

    Task<PlayerSummaryDto?> GetPlayerById(ILogger<PlayerSummaryDto> logger, int playerId);
    Task<PlayerStatsDto?> GetPlayerStats(ILogger<PlayerSummaryDto> logger, int playerId);

    Task<int> CreatePlayer(
        SqlConnection connection,
        SqlTransaction transaction,
        ILogger logger,
        PlayerUpsertDto player);

    Task<bool> UpdatePlayer(
        SqlConnection connection,
        SqlTransaction transaction,
        ILogger logger,
        int playerId,
        PlayerUpsertDto player);

    Task<bool> DeletePlayer(
        SqlConnection connection,
        SqlTransaction transaction,
        ILogger logger,
        int playerId);

    Task<string?> GetPlayerSportCode(
        SqlConnection connection,
        SqlTransaction transaction,
        int playerId);

    Task<bool> UpsertPlayerStats(
        SqlConnection connection,
        SqlTransaction transaction,
        ILogger logger,
        int playerId,
        string sportCode,
        PlayerStatsUpsertDto stats);

    Task DeletePlayerStats(
        SqlConnection connection,
        SqlTransaction transaction,
        ILogger logger,
        int playerId,
        string sportCode);
}
