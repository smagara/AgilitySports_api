using AgilitySportsAPI.Models;
using Dapper.Contrib.Extensions;
using Microsoft.Data.SqlClient;
using AgilitySportsAPI.Dtos;
using Dapper;

namespace AgilitySportsAPI.Data;
public class NHLRepo : BaseRepo, INHLRepo
{
    public NHLRepo(IConfiguration configuration) : base(configuration)
    {
    }

    #region NHL

    public async Task<IEnumerable<NHLRosterDto>?> GetNHLRoster(ILogger<NHLRoster> logger, int? playerId)
    {
        try
        {
            logger.LogInformation("Fetching NHL Roster");

            var sql = @"
                    select 
                        Name
                        ,Team
                        ,Number
                        ,Position
                        ,Handed
                        ,Age
                        ,Drafted
                        ,BirthPlace
                        ,BirthCountry
                        ,playerID
                    from NHL.Roster
                    where 
                        (@playerId is null or playerID = @playerId)
                    order by 
                    1, 3, 2";
            using (var connection = new SqlConnection(base.connectionString))
            {
                await base.GenToken(connection);
                return await connection.QueryAsync<NHLRosterDto>(sql, new { playerId = playerId });
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Error creating NHL player: " + ex.Message);
            return null;
        }
    }

    public async Task<NHLRoster?> CreateNHLRoster(NHLRoster roster, ILogger<NHLRoster> logger)
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
            logger.LogError("Error creating NHL player: " + ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Updates an NHL roster entry for the specified player.  the roster entry contains all dto fields.
    /// </summary>
    /// <param name="roster">NHL roster instance to perform the update.</param>
    /// <param name="logger">Logger for logging operations.</param>
    /// <returns>A task that represents the asynchronous update operation. The task result contains a boolean indicating whether the update was successful.</returns>
    public async Task<bool> UpdateNHLRoster(NHLRoster roster, ILogger<NHLRoster> logger)
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
            logger.LogError("Error updating NHL Roster: " + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Deletes an NHL roster entry for the specified player ID.
    /// </summary>
    /// <param name="playerId">The ID of the player whose roster entry is to be deleted.</param>
    /// <param name="logger">Logger for logging operations.</param>
    /// <returns>A task that represents the asynchronous delete operation. The task result contains a boolean indicating whether the delete was successful.</returns>
    public async Task<bool> DeleteNHLRoster(int playerId, ILogger<NHLRoster> logger)
    {
        try
        {
            using var connection = new SqlConnection(base.connectionString);
            await base.GenToken(connection);
            var roster = await connection.GetAsync<NHLRoster>(playerId);
            if (roster != null)
            {
                await connection.DeleteAsync(roster);
            }
            else
            {
                logger.LogError($"Unable to Delete: NHL Roster playerId {playerId} not found");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Error deleting NHL Roster: " + ex.Message);
            return false;
        }
    }

    #endregion
}