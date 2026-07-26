using AgilitySportsAPI.Models;
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
                        concat(p.firstName, ' ', p.lastName) as Name
                        ,p.firstName as FirstName
                        ,p.lastName as LastName
                        ,p.teamCode as TeamCode
                        ,p.teamCode as Team
                        ,coalesce(t.teamShortName, t.teamName, p.teamCode) as TeamName
                        ,t.league as League
                        ,convert(varchar(10), p.Number) as Number
                        ,coalesce(pc.positionDesc, p.positionCode) as Position
                        ,convert(varchar(10), p.heightInches) as Height
                        ,convert(varchar(10), p.weight) as Weight
                        ,p.dateOfBirth as DateOfBirth
                        ,try_convert(tinyint, datediff(year, p.dateOfBirth, getdate())) as Age
                        ,p.draftYear as DraftYear
                        ,p.seasonYear as SeasonYear
                        ,p.college as College
                        ,p.birthCityState as BirthCityState
                        ,p.birthCountry as BirthCountry
                        ,p.playerID as PlayerId
                        ,nhl.handed as Handed
                        ,nhl.goals as Goals
                        ,nhl.penaltyMinutes as PenaltyMinutes
                        ,nhl.points as Points
                        ,nhl.savePct as SavePct
                    from core.Players p
                    left join core.Teams t
                        on t.sportCode = p.sportCode
                        and t.teamCode = p.teamCode
                    left join reference.PositionCodes pc
                        on pc.sportCode = p.sportCode
                        and pc.positionCode = p.positionCode
                    left join stats.NHLPlayerStats nhl
                        on nhl.sportCode = p.sportCode
                        and nhl.playerID = p.playerID
                    where p.sportCode = 'NHL'
                        and (@playerId is null or p.playerID = @playerId)
                    order by 
                        p.playerID, p.lastName, p.firstName";
            using (var connection = new SqlConnection(base.connectionString))
            {
                await base.GenToken(connection);
                return await connection.QueryAsync<NHLRosterDto>(sql, new { playerId = playerId });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching NHL player(s).");
            return null;
        }
    }

    public async Task<NHLRoster?> CreateNHLRoster(NHLRoster roster, ILogger<NHLRoster> logger)
    {
        logger.LogError("Legacy NHL roster write endpoints are not supported on DB V2. Use /api/v2/players.");
        await Task.CompletedTask;
        return null;
    }

    /// <summary>
    /// Updates an NHL roster entry for the specified player.  the roster entry contains all dto fields.
    /// </summary>
    /// <param name="roster">NHL roster instance to perform the update.</param>
    /// <param name="logger">Logger for logging operations.</param>
    /// <returns>A task that represents the asynchronous update operation. The task result contains a boolean indicating whether the update was successful.</returns>
    public async Task<bool> UpdateNHLRoster(NHLRoster roster, ILogger<NHLRoster> logger)
    {
        logger.LogError("Legacy NHL roster write endpoints are not supported on DB V2. Use /api/v2/players.");
        await Task.CompletedTask;
        return false;
    }

    /// <summary>
    /// Deletes an NHL roster entry for the specified player ID.
    /// </summary>
    /// <param name="playerId">The ID of the player whose roster entry is to be deleted.</param>
    /// <param name="logger">Logger for logging operations.</param>
    /// <returns>A task that represents the asynchronous delete operation. The task result contains a boolean indicating whether the delete was successful.</returns>
    public async Task<bool> DeleteNHLRoster(int playerId, ILogger<NHLRoster> logger)
    {
        logger.LogError("Legacy NHL roster write endpoints are not supported on DB V2. Use /api/v2/players.");
        await Task.CompletedTask;
        return false;
    }

    #endregion
}