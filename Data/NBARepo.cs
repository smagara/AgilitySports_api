using AgilitySportsAPI.Models;
using Dapper.Contrib.Extensions;
using Microsoft.Data.SqlClient;
using AgilitySportsAPI.Dtos;
using Dapper;

namespace AgilitySportsAPI.Data;
public class NBARepo : INBARepo
{
    private readonly IConfiguration configuration;

    public NBARepo(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public async Task<IEnumerable<NBARoster>> GetAllNBARoster()
    {
        using (var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection")))
        {
            return await connection.GetAllAsync<NBARoster>();
        }

    }

        public async Task<IEnumerable<NBARosterDto>> GetNBARoster(ILogger<NBARoster> logger)
    {
        logger.LogInformation("Fetching NBA Roster");
 
                var sql = @"
select playerID
      ,FirstName
      ,LastName
      ,Team
      ,Position
      ,Number
      ,Height
      ,Weight
      ,DateOfBirth
      ,College
from NBA.Roster
order by 
  1, 3, 2";
        using (var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection")))
        {
            return await connection.QueryAsync<NBARosterDto>(sql);
        }
    }

}