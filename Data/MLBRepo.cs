using AgilitySportsAPI.Models;
using Dapper.Contrib.Extensions;
using Microsoft.Data.SqlClient;
using AgilitySportsAPI.Dtos;
using Dapper;

namespace AgilitySportsAPI.Data;
public class MLBRepo : IMLBRepo
{
    private readonly IConfiguration configuration;

    public MLBRepo(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    #region NHL

    public async Task<IEnumerable<MLBRoster>> GetAllMLBRoster()
    {
        using (var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection")))
        {
            return await connection.GetAllAsync<MLBRoster>();
        }

    }

        public async Task<IEnumerable<MLBRosterDto>> GetMLBRoster()
    {
                var sql = @"
select 
    PlayerId
    ,FirstName
    ,LastName
    ,TeamName
    ,Position
    ,Bats
    ,Throws
    ,DateOfBirth
    ,Height
    ,Weight
    ,League
    ,BirthPlace
    ,BirthCountry
from MLB.Roster
order by 
  3,2";
        using (var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection")))
        {
            return await connection.QueryAsync<MLBRosterDto>(sql);
        }
    }

    #endregion
}