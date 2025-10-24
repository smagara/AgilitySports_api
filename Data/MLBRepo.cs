using AgilitySportsAPI.Models;
using Dapper.Contrib.Extensions;
using Microsoft.Data.SqlClient;
using AgilitySportsAPI.Dtos;
using Dapper;
//using System.Text.Json;
using AgilitySportsAPI.Utilities;
using AgilitySportsAPI.Services;


namespace AgilitySportsAPI.Data;
public class MLBRepo : BaseRepo, IMLBRepo
{
    private readonly IColorWheel colors;
    private readonly IRosterExistenceService _existenceService;

    public MLBRepo(ILogger<MLBRoster> logger, IConfiguration configuration, IColorWheel colors, IRosterExistenceService existenceService)
        : base(configuration)
    {
        this.colors = colors;
        _existenceService = existenceService;
    }
    // Example update method for MLB Roster
    public async Task<bool> UpdateMLBRoster(MLBRoster roster, ILogger<MLBRoster> logger)
    {
        logger.LogInformation("Updating MLB Roster");
        try
        {
            if (string.IsNullOrEmpty(roster.PlayerID))
            {
                logger.LogWarning("MLB Roster update failed: PlayerID is null or empty.");
                return false;
            }
            if (!await _existenceService.ExistsAsync<MLBRoster>(roster.PlayerID, base.connectionString))
            {
                logger.LogWarning($"MLB Roster with PlayerID {roster.PlayerID} not found.");
                return false;
            }
            using (var connection = new SqlConnection(base.connectionString))
            {
                await base.GenToken(connection);
                await connection.UpdateAsync(roster);
                return true;
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Error updating MLB Roster: " + ex.Message);
            return false;
        }
    }

    #region MLB.Roster

    public async Task<IEnumerable<MLBRoster>> GetAllMLBRoster()
    {
        using (var connection = new SqlConnection(base.connectionString))
        {
            await base.GenToken(connection);
            return await connection.GetAllAsync<MLBRoster>();
        }

    }

    public async Task<IEnumerable<MLBRosterDto>> GetMLBRoster(ILogger<MLBRoster> logger)
    {
        var sql = @"
            select 
                PlayerID
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
        using (var connection = new SqlConnection(base.connectionString))
        {
            await base.GenToken(connection);
            return await connection.QueryAsync<MLBRosterDto>(sql);
        }
    }

    #endregion

    #region MLB.Attendance
    public async Task<IEnumerable<MLBAttendance>> GetAllMLBAttendance()
    {
        using (var connection = new SqlConnection(base.connectionString))
        {
            await base.GenToken(connection);
            return await connection.GetAllAsync<MLBAttendance>();
        }
    }

    public async Task<IEnumerable<MLBAttendanceDto>> GetMLBAttendance(ILogger<MLBAttendanceDto> logger, short? year = null)
    {
        logger.LogInformation($"Fetching MLB Attendance Grid for year {year}");

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

        using (var connection = new SqlConnection(base.connectionString))
        {
            await base.GenToken(connection);
            return await connection.QueryAsync<MLBAttendanceDto>(sql, new {yearId=year, year});
        }
    }
    #endregion

    #region chart
 
    // construct a PrimeNG chart data feed to bring the data to life
    public async Task<MLBAttendChartDTO> GetMLBChart(ILogger<MLBAttendChartDTO> logger, short? year)
    {
        MLBAttendChartDTO mlbChart = new MLBAttendChartDTO();
        logger.LogInformation("Fetching MLB Attendance Chart for year {year}", year);

        try
        {
        using (var connection = new SqlConnection(base.connectionString))
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
            await base.GenToken(connection);
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
        }
        catch(Exception ex)
        {
            logger.LogError("Error fetching MLB Attendance chart JSON: {theError}", ex.Message);
        }
        return mlbChart;

    }

    // construct a PrimeNG chart data feed for attendance over the decades
    public async Task<MLBAttendChartDTO> GetMLBDecades(ILogger<MLBAttendChartDTO> logger, short? beginDecade = 1920, short? endDecade = 2010)
    {
        MLBAttendChartDTO mlbDecs = new MLBAttendChartDTO();
        logger.LogInformation("Fetching MLB Decade Attendance for years {beginDecade} to {endDecade}", beginDecade, endDecade);
        try
        {
            using (var connection = new SqlConnection(base.connectionString))
            {
                var sql = @"
                EXEC MLB.[attendanceReportSproc] @begin, @end;";
                // begin to assemble our chart payload
                mlbDecs.datasets = new List<Dataset>();
                // run the query
                await base.GenToken(connection);
                IEnumerable<MLBAttendanceDto> chartData = await connection.QueryAsync<MLBAttendanceDto>(sql, new { begin = beginDecade, end = endDecade });
                foreach (var dec in chartData)
                {
                    Dataset myChartData = new Dataset
                    {
                        label = dec.YearId.ToString() + "'s" ?? "",
                        backgroundColor = colors.Next(),
                        borderColor = "darkgray",
                        borderWidth = "1",
                        data = new List<string> { dec.Attendance?.ToString() ?? "" }
                    };
                    mlbDecs.datasets.Add(myChartData);
                }
            }
            mlbDecs.labels = new List<string> { "Baseball Attendance " + beginDecade + "'s -- " + endDecade + "'s" };
            //Console.WriteLine("GetMLBChart " +  JsonSerializer.Serialize (mlbDecs));
        }
        catch (Exception ex)
        {
            logger.LogError("Error fetching MLB Attendance Decades chart JSON: {theError}", ex.Message);
        }
        return mlbDecs;
    }

    #endregion
}