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

    #region MLB.Roster

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

    #region MLB.Attendance
    public async Task<IEnumerable<MLBAttendance>> GetAllMLBAttendance()
    {
        using (var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection")))
        {
            return await connection.GetAllAsync<MLBAttendance>();
        }
    }

    public async Task<IEnumerable<MLBAttendanceDto>> GetMLBAttendance()
    {
        var sql = @"
        select 
            yearId
            ,teamId
            ,teamName
            ,parkName
            ,attendance
        from MLB.Attendance
        order by yearId, teamId";
        using (var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection")))
        {
            return await connection.QueryAsync<MLBAttendanceDto>(sql);
        }
    }
    #endregion
}