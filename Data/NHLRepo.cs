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

    public async Task<IEnumerable<NHLRoster>> GetAllNHLRoster()
    {
        using (var connection = new SqlConnection(base.connectionString))
        {
            await base.GenToken(connection);
            return await connection.GetAllAsync<NHLRoster>();
        }

    }

    public async Task<IEnumerable<NHLRosterDto>> GetNHLRoster(ILogger<NHLRoster> logger)
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
                    from NHL.Roster
                    order by 
                    1, 3, 2";
        using (var connection = new SqlConnection(base.connectionString))
        {
            await base.GenToken(connection);
            return await connection.QueryAsync<NHLRosterDto>(sql);
        }
    }

    #endregion
}