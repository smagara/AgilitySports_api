using AgilitySportsAPI.Models;
using Microsoft.Data.SqlClient;
using AgilitySportsAPI.Dtos;
using Dapper;

namespace AgilitySportsAPI.Data;

public class FIFRepo : BaseRepo, IFIFRepo
{
    public FIFRepo(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<IEnumerable<FIFRosterDto>?> GetFIFRoster(ILogger<FIFRoster> logger, int? playerId)
    {
        try
        {
            logger.LogInformation("Fetching FIF Roster");

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
                ,fif.totalGoals as TotalGoals
                ,fif.assists as Assists
                ,fif.saves as Saves
            from core.Players p
            left join core.Teams t
                on t.sportCode = p.sportCode
                and t.teamCode = p.teamCode
            left join reference.PositionCodes pc
                on pc.sportCode = p.sportCode
                and pc.positionCode = p.positionCode
            left join stats.FIFPlayerStats fif
                on fif.playerID = p.playerID
                and fif.sportCode = p.sportCode
            where p.sportCode = 'FIF'
                and (@playerId is null or p.playerID = @playerId)
            order by
                p.playerID, p.lastName, p.firstName";

            using (var connection = new SqlConnection(base.connectionString))
            {
                await base.GenToken(connection);
                return await connection.QueryAsync<FIFRosterDto>(sql, new { playerId });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching FIF roster.");
            return null;
        }
    }

    public async Task<FIFRoster?> CreateFIFRoster(FIFRoster roster, ILogger<FIFRoster> logger)
    {
        logger.LogError("Legacy FIF roster write endpoints are not supported on DB V2. Use /api/v2/players.");
        return null;
    }

    public async Task<bool> UpdateFIFRoster(FIFRoster roster, ILogger<FIFRoster> logger)
    {
        logger.LogError("Legacy FIF roster write endpoints are not supported on DB V2. Use /api/v2/players.");
        await Task.CompletedTask;
        return false;
    }

    public async Task<bool> DeleteFIFRoster(int playerId, ILogger<FIFRoster> logger)
    {
        logger.LogError("Legacy FIF roster write endpoints are not supported on DB V2. Use /api/v2/players.");
        await Task.CompletedTask;
        return false;
    }
}
