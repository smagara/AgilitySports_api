using AgilitySportsAPI.Models;
using Dapper.Contrib.Extensions;
using Microsoft.Data.SqlClient;
using AgilitySportsAPI.Dtos;
using Dapper;

namespace AgilitySportsAPI.Data;
public class NBARepo : BaseRepo, INBARepo
{
    public NBARepo(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<IEnumerable<NBARosterDto>?> GetNBARoster(ILogger<NBARoster> logger, int? playerId)
    {
        try
        {
            logger.LogInformation("Fetching NBA Roster");

            var sql = @"
            select playerID
                ,FirstName
                ,LastName
                ,Team
                ,Position
                ,Number
                ,Height
                ,Weight
                ,DateOfBirth
                ,College
            from NBA.Roster
            where 
                (@playerId is null or playerID = @playerId)
            order by 
            1, 3, 2";

            using (var connection = new SqlConnection(base.connectionString))
            {
                await base.GenToken(connection);
                return await connection.QueryAsync<NBARosterDto>(sql, new { playerId = playerId });
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Error creating NHL player: " + ex.Message);
            return null;
        }
    }

    #region updateCrud
    public async Task<NBARoster?> CreateNBARoster(NBARoster roster, ILogger<NBARoster> logger)
    {
        try
        {
            using (var connection = new SqlConnection(base.connectionString))
            {
                await base.GenToken(connection);
                await connection.InsertAsync(roster);
                return roster;
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Error creating NBA player: " + ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Updates an NBA roster entry for the specified player.  the roster entry contains all dto fields.
    /// </summary>
    /// <param name="roster">NBA roster instance to perform the update.</param>
    /// <param name="logger">Logger for logging operations.</param>
    /// <returns>A task that represents the asynchronous update operation. The task result contains a boolean indicating whether the update was successful.</returns>
    public async Task<bool> UpdateNBARoster(NBARoster roster, ILogger<NBARoster> logger)
    {
        try
        {
            using (var connection = new SqlConnection(base.connectionString))
            {
                await base.GenToken(connection);
                await connection.UpdateAsync(roster);
                return true;
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Error updating NBA Roster: " + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Deletes an NBA roster entry for the specified player ID.
    /// </summary>
    /// <param name="playerId">The ID of the player whose roster entry is to be deleted.</param>
    /// <param name="logger">Logger for logging operations.</param>
    /// <returns>A task that represents the asynchronous delete operation. The task result contains a boolean indicating whether the delete was successful.</returns>
    public async Task<bool> DeleteNBARoster(int playerId, ILogger<NBARoster> logger)
    {
        try
        {
            using var connection = new SqlConnection(base.connectionString);
            await base.GenToken(connection);
            var roster = await connection.GetAsync<NBARoster>(playerId);
            if (roster != null)
            {
                await connection.DeleteAsync(roster);
            }
            else
            {
                logger.LogError($"Unable to Delete: NBA Roster playerId {playerId} not found");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Error deleting NBA Roster: " + ex.Message);
            return false;
        }
    }

    #endregion
}