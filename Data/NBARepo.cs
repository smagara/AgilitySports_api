using AgilitySportsAPI.Models;
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
            select p.playerID as PlayerId
                ,p.firstName as FirstName
                ,p.lastName as LastName
                ,p.teamCode as TeamCode
                ,p.teamCode as Team
                ,t.teamShortName as TeamName
                ,t.league as League
                ,coalesce(pc.positionDesc, p.positionCode) as Position
                ,convert(varchar(10), p.number) as Number
                ,convert(varchar(10), p.heightInches) as Height
                ,convert(varchar(10), p.weight) as Weight
                ,p.dateOfBirth as DateOfBirth
                ,p.college as College
                ,p.birthCityState as BirthCityState
                ,p.birthCountry as BirthCountry
                ,p.draftYear as DraftYear
                ,p.seasonYear as SeasonYear
            from core.Players p
            left join core.Teams t
                on t.sportCode = p.sportCode
                and t.teamCode = p.teamCode
            left join reference.PositionCodes pc
                on pc.sportCode = p.sportCode
                and pc.positionCode = p.positionCode
            where p.sportCode = 'NBA'
                and (@playerId is null or p.playerID = @playerId)
            order by 
                p.playerID, p.lastName, p.firstName";

            using (var connection = new SqlConnection(base.connectionString))
            {
                await base.GenToken(connection);
                return await connection.QueryAsync<NBARosterDto>(sql, new { playerId = playerId });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating NBA player.");
            return null;
        }
    }

    #region updateCrud
    public async Task<NBARoster?> CreateNBARoster(NBARoster roster, ILogger<NBARoster> logger)
    {
        logger.LogError("Legacy NBA roster write endpoints are not supported on DB V2. Use /api/v2/players.");
        return null;
    }

    /// <summary>
    /// Updates an NBA roster entry for the specified player.  the roster entry contains all dto fields.
    /// </summary>
    /// <param name="roster">NBA roster instance to perform the update.</param>
    /// <param name="logger">Logger for logging operations.</param>
    /// <returns>A task that represents the asynchronous update operation. The task result contains a boolean indicating whether the update was successful.</returns>
    public async Task<bool> UpdateNBARoster(NBARoster roster, ILogger<NBARoster> logger)
    {
        logger.LogError("Legacy NBA roster write endpoints are not supported on DB V2. Use /api/v2/players.");
        await Task.CompletedTask;
        return false;
    }

    /// <summary>
    /// Deletes an NBA roster entry for the specified player ID.
    /// </summary>
    /// <param name="playerId">The ID of the player whose roster entry is to be deleted.</param>
    /// <param name="logger">Logger for logging operations.</param>
    /// <returns>A task that represents the asynchronous delete operation. The task result contains a boolean indicating whether the delete was successful.</returns>
    public async Task<bool> DeleteNBARoster(int playerId, ILogger<NBARoster> logger)
    {
        logger.LogError("Legacy NBA roster write endpoints are not supported on DB V2. Use /api/v2/players.");
        await Task.CompletedTask;
        return false;
    }

    #endregion
}