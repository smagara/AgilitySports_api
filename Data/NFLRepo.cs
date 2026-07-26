using AgilitySportsAPI.Models;
using Microsoft.Data.SqlClient;
using AgilitySportsAPI.Dtos;
using Dapper;

namespace AgilitySportsAPI.Data;
public class NFLRepo : BaseRepo, INFLRepo
{
    public NFLRepo(IConfiguration configuration) : base(configuration)
    {
    }

    #region NFL

    public async Task<IEnumerable<NFLRosterDto>?> GetNFLRoster(ILogger<NFLRoster> logger, int? playerId)
    {
        logger.LogInformation("Fetching NFL Roster");
        try
        {
            var sql = @"
                    select 
                        p.teamCode as TeamCode
                        ,p.teamCode as Team
                        ,coalesce(t.teamShortName, t.teamName, p.teamCode) as TeamName
                        ,t.league as League
                        ,p.firstName
                        ,p.lastName
                        ,coalesce(pc.positionDesc, p.positionCode) as Position
                        ,convert(varchar(10), p.number) as Number
                        ,convert(varchar(10), p.heightInches) as Height
                        ,convert(varchar(10), p.weight) as Weight
                        ,p.dateOfBirth as DateOfBirth
                        ,try_convert(tinyint, datediff(year, p.dateOfBirth, getdate())) as Age
                        ,p.college as College
                        ,p.birthCityState as BirthCityState
                        ,p.birthCountry as BirthCountry
                        ,p.draftYear as DraftYear
                        ,p.seasonYear as SeasonYear
                        ,p.playerId as PlayerId
                        ,nfl.sacks as Sacks
                        ,nfl.touchdowns as Touchdowns
                    from core.Players p
                    left join core.Teams t
                        on t.sportCode = p.sportCode
                        and t.teamCode = p.teamCode
                    left join reference.PositionCodes pc
                        on pc.sportCode = p.sportCode
                        and pc.positionCode = p.positionCode
                    left join stats.NFLPlayerStats nfl
                        on nfl.playerID = p.playerID
                        and nfl.sportCode = p.sportCode
                    where p.sportCode = 'NFL'
                        and (@playerId is null or p.playerId = @playerId)
                    order by 
                        p.playerId, p.lastName, p.firstName";
            using (var connection = new SqlConnection(base.connectionString))
            {
                await base.GenToken(connection);
                return await connection.QueryAsync<NFLRosterDto>(sql, new { playerId });
            }

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching NFL roster.");
            return null;
        }
    }

    public async Task<NFLRoster?> Create(NFLRoster player, ILogger<NFLRoster> logger)
    {
        logger.LogError("Legacy NFL roster write endpoints are not supported on DB V2. Use /api/v2/players.");
        await Task.CompletedTask;
        return null;
    }

    public async Task<bool> Update(NFLRoster player, ILogger<NFLRoster> logger)
    {
        logger.LogError("Legacy NFL roster write endpoints are not supported on DB V2. Use /api/v2/players.");
        await Task.CompletedTask;
        return false;
    }

    public async Task<bool> Delete(int playerId, ILogger<NFLRoster> logger)
    {
        logger.LogError("Legacy NFL roster write endpoints are not supported on DB V2. Use /api/v2/players.");
        await Task.CompletedTask;
        return false;
    }

    #endregion
}