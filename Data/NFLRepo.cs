using AgilitySportsAPI.Models;
using Dapper.Contrib.Extensions;
using Microsoft.Data.SqlClient;
using AgilitySportsAPI.Dtos;
using Dapper;
using AgilitySportsAPI.Services;

namespace AgilitySportsAPI.Data;
public class NFLRepo : BaseRepo, INFLRepo
{
    private readonly IRosterExistenceService _existenceService;

    public NFLRepo(IConfiguration configuration, IRosterExistenceService existenceService) : base(configuration)
    {
        _existenceService = existenceService;
    }

    #region NFL

    public async Task<IEnumerable<NFLRosterDto>?> GetNFLRoster(ILogger<NFLRoster> logger, int? playerId)
    {
        logger.LogInformation("Fetching NFL Roster");
        try
        {
            var sql = @"
                    select 
                    Team
                    , firstName
                    , lastName
                    , Position
                    , Number
                    , Height
                    , Weight
                    , Age
                    , College
                    , playerId
                    from NFL.Roster
                    where 
                        (@playerId is null or playerId = @playerId)
                    order by 
                    1, 3, 2";
            using (var connection = new SqlConnection(base.connectionString))
            {
                await base.GenToken(connection);
                return await connection.QueryAsync<NFLRosterDto>(sql, new { playerId });
            }

        }
        catch (Exception ex)
        {
            logger.LogError("Error fetching NFL roster: " + ex.Message);
            return null;
        }
    }

    public async Task<NFLRoster?> Create(NFLRoster player, ILogger<NFLRoster> logger)
    {
        logger.LogInformation("Creating NFL Roster");
        try
        {
            using (var connection = new SqlConnection(base.connectionString))
            {
                await base.GenToken(connection);
                await connection.InsertAsync(player);
                return player;
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Error creating NFL Roster: " + ex.Message);
            return null;
        }
    }

    public async Task<bool> Update(NFLRoster player, ILogger<NFLRoster> logger)
    {
        logger.LogInformation("Updating NFL Roster");
        try
        {
            if (!await _existenceService.ExistsAsync<NFLRoster>(player.PlayerId, base.connectionString))
            {
                logger.LogWarning($"NFL Roster with PlayerID {player.PlayerId} not found.");
                return false;
            }
            using (var connection = new SqlConnection(base.connectionString))
            {
                await base.GenToken(connection);
                return await connection.UpdateAsync(player);
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Error updating NFL Roster: " + ex.Message);
            return false;
        }
    }

    public async Task<bool> Delete(int playerId, ILogger<NFLRoster> logger)
    {
        logger.LogInformation("Deleting NFL Roster");
        try
        {
            if (!await _existenceService.ExistsAsync<NFLRoster>(playerId, base.connectionString))
            {
                logger.LogError($"Unable to Delete: NFL Roster playerId {playerId} not found");
                return false;
            }
            using (var connection = new SqlConnection(base.connectionString))
            {
                await base.GenToken(connection);
                var player = await connection.GetAsync<NFLRoster>(playerId);
                await connection.DeleteAsync(player);
                return true;
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Error deleting NFL Roster: " + ex.Message);
            return false;
        }
    }

    #endregion
}