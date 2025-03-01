using AgilitySportsAPI.Models;
using Dapper.Contrib.Extensions;
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
                    Team
                    , Name
                    , Position
                    , Number
                    , Height
                    , Weight
                    , AgeExact
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

    #endregion
}