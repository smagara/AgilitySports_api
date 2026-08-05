using AgilitySportsAPI.Models;
using Microsoft.Data.SqlClient;
using AgilitySportsAPI.Dtos;
using Dapper;

namespace AgilitySportsAPI.Data;

public class PGARepo : BaseRepo, IPGARepo
{
    public PGARepo(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<IEnumerable<PGARosterDto>?> GetPGARoster(ILogger<PGARoster> logger, int? playerId)
    {
        try
        {
            logger.LogInformation("Fetching PGA Roster");

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
                ,pga.wins as Wins
                ,pga.majors as Majors
                ,pga.drivingDistance as DrivingDistance
                ,pga.scoringAverage as ScoringAverage
                ,pga.eventsPlayed as EventsPlayed
                ,pga.cutsMade as CutsMade
            from core.Players p
            left join core.Teams t
                on t.sportCode = p.sportCode
                and t.teamCode = p.teamCode
            left join reference.PositionCodes pc
                on pc.sportCode = p.sportCode
                and pc.positionCode = p.positionCode
            left join stats.PGAPlayerStats pga
                on pga.playerID = p.playerID
                and pga.sportCode = p.sportCode
            where p.sportCode = 'PGA'
                and (@playerId is null or p.playerID = @playerId)
            order by
                p.playerID, p.lastName, p.firstName";

            using (var connection = new SqlConnection(base.connectionString))
            {
                await base.GenToken(connection);
                return await connection.QueryAsync<PGARosterDto>(sql, new { playerId });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching PGA roster.");
            return null;
        }
    }

    public async Task<PGARoster?> CreatePGARoster(PGARoster roster, ILogger<PGARoster> logger)
    {
        logger.LogError("Legacy PGA roster write endpoints are not supported on DB V2. Use /api/v2/players.");
        return null;
    }

    public async Task<bool> UpdatePGARoster(PGARoster roster, ILogger<PGARoster> logger)
    {
        logger.LogError("Legacy PGA roster write endpoints are not supported on DB V2. Use /api/v2/players.");
        await Task.CompletedTask;
        return false;
    }

    public async Task<bool> DeletePGARoster(int playerId, ILogger<PGARoster> logger)
    {
        logger.LogError("Legacy PGA roster write endpoints are not supported on DB V2. Use /api/v2/players.");
        await Task.CompletedTask;
        return false;
    }
}
