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

    public async Task<IEnumerable<NFLRoster>> GetAllNFLRoster()
    {
        using (var connection = new SqlConnection(base.connectionString))
        {
            await base.GenToken(connection);
            return await connection.GetAllAsync<NFLRoster>();
        }
    }

    public async Task<IEnumerable<NFLRosterDto>> GetNFLRoster(ILogger<NFLRepo> logger)
    {
        logger.LogInformation("Fetching NFL Roster");

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
                    from NFL.Roster
                    order by 
                    1, 3, 2";
        using (var connection = new SqlConnection(base.connectionString))
        {
            await base.GenToken(connection);
            return await connection.QueryAsync<NFLRosterDto>(sql);
        }
    }

    #endregion
}