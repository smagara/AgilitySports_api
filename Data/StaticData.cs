using AgilitySportsAPI.Dtos;
using Microsoft.Data.SqlClient;
using Dapper;

namespace AgilitySportsAPI.Data;

public class StaticData : BaseRepo, IStaticData
{

    public StaticData(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<IEnumerable<PositionCodesDTO>?> GetPositionCodes(ILogger<PositionCodesDTO> logger, string? sport)
    {
        try
        {
            logger.LogInformation("Fetching Position Codes");
            var normalizedSport = string.IsNullOrWhiteSpace(sport) ? null : sport.Trim().ToUpperInvariant();

            var sql = @"
                    select 
                    sportCode as SportCode
                    ,positionCode as PositionCode
                    ,positionDesc as PositionDesc
                    from reference.PositionCodes
                    where 
                    (@sport is null or sportCode = @sport)
                    order by 
                    1, 2, 3";
            using (var connection = new SqlConnection(base.connectionString))
            {
                await base.GenToken(connection);
                return await connection.QueryAsync<PositionCodesDTO>(sql, new { sport = normalizedSport });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching Position codes for the Sport:  " + sport + ".");
            return null;
        }
    }

    public async Task<IEnumerable<TeamLeagueDto>?> GetTeamLeagues(ILogger<TeamLeagueDto> logger, string? sport)
    {
        try
        {
            logger.LogInformation("Fetching Team/League static data");
            var normalizedSport = string.IsNullOrWhiteSpace(sport) ? null : sport.Trim().ToUpperInvariant();

            var sql = @"
                    select
                        sportCode as Sport
                        ,teamCode as TeamCode
                        ,coalesce(teamShortName, teamName, teamCode) as TeamName
                        ,league as League
                    from core.Teams
                    where
                        (@sport is null or sportCode = @sport)
                    order by
                        sportCode, teamCode";

            using (var connection = new SqlConnection(base.connectionString))
            {
                await base.GenToken(connection);
                return await connection.QueryAsync<TeamLeagueDto>(sql, new { sport = normalizedSport });
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Error fetching team static data for sport {Sport}: {Message}", sport, ex.Message);
            return null;
        }
    }
}
