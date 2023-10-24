using AgilitySportsAPI.Models;
using Dapper.Contrib.Extensions;
using Microsoft.Data.SqlClient;
using AgilitySportsAPI.Dtos;
using Dapper;
//using System.Text.Json;
using AgilitySportsAPI.Utilities;


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

    public async Task<IEnumerable<MLBAttendanceDto>> GetMLBAttendance(short? year = null)
    {
        var sql = @"
        select 
            yearId
            ,teamId
            ,teamName
            ,parkName
            ,attendance
        from MLB.Attendance 
        where (@yearId IS NULL OR yearId = @yearId)
        order by yearId, teamId";

        using (var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection")))
        {
            return await connection.QueryAsync<MLBAttendanceDto>(sql, new {yearId=year, year});
        }
    }
    #endregion

    #region chart
 
    // construct a PrimeNG chart data feed to bring the data to life
    public async Task<MLBAttendChartDTO> GetMLBChart(short? year)
    {
        MLBAttendChartDTO mlbChart = new MLBAttendChartDTO();
        ColorWheel colors = new ColorWheel();

        using (var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection")))
        {
            var sql = @"
                select 
                    yearId
                    ,teamName
                    ,attendance
                from MLB.Attendance 
                where @yearId IS NULL OR yearId = @yearId
                Order by attendance desc";

            // begin to assemble our chart payload
            mlbChart.datasets = new List<Dataset>();

            // run the query
            IEnumerable<MLBAttendanceDto> chartData = await connection.QueryAsync<MLBAttendanceDto>(sql, new {yearId=year, year});

            foreach(var team in chartData)
            {
                Dataset myChartData = new Dataset
                {
                    label = team.TeamName ?? "",
                    backgroundColor = colors.Next(),
                    borderColor = "darkgray",
                    borderWidth = "1",
                    data = new List<string>{team.Attendance?.ToString() ?? ""}
                };

                mlbChart.datasets.Add(myChartData);
            }
         }

        mlbChart.labels = new List<string> { "Baseball Attendance " + year ?? "" };
        //Console.WriteLine("GetMLBChart " +  JsonSerializer.Serialize (mlbChart));

        return mlbChart;

    }    
    #endregion
}