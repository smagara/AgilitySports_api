using AgilitySportsAPI.Models;
using Dapper.Contrib.Extensions;
using Microsoft.Data.SqlClient;
using AgilitySportsAPI.Dtos;
using Dapper;

namespace AgilitySportsAPI.Data;

public class StaticData : BaseRepo, IStaticData
{

    public StaticData(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<IEnumerable<PositionCodes>?> GetPositionCodes(ILogger<PositionCodes> logger, string sport)
    {
        try
        {
            logger.LogInformation("Fetching Position Codes");

            var sql = @"
                    select 
                        Sport
                        ,PositionCode
                        ,PositionDesc
                    from dbo.PositionCodes
                    where 
                        (@sport is null or Sport = @sport)
                    order by 
                    1, 2, 3";
            using (var connection = new SqlConnection(base.connectionString))
            {
                await base.GenToken(connection);
                return await connection.QueryAsync<PositionCodes>(sql, new { sport = sport });
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Error fetching Position codes for the Sport:  " + sport + " : " + ex.Message);
            return null;
        }
    }
}
