using AgilitySportsAPI.Models;
using Microsoft.Data.SqlClient;
using AgilitySportsAPI.Dtos;
using Dapper;
//using System.Text.Json;
using AgilitySportsAPI.Utilities;


namespace AgilitySportsAPI.Data;
public class MLBRepo : BaseRepo, IMLBRepo
{
    private readonly IColorWheel colors;

    public MLBRepo(ILogger<MLBRoster> logger, IConfiguration configuration, IColorWheel colors)
        : base(configuration)
    {
        this.colors = colors;
    }
    // Example update method for MLB Roster
    public async Task<bool> UpdateMLBRoster(MLBRoster roster, ILogger<MLBRoster> logger)
    {
        logger.LogError("Legacy MLB roster write endpoints are not supported on DB V2. Use /api/v2/players.");
        await Task.CompletedTask;
        return false;
    }

    #region MLB.Roster

    public async Task<IEnumerable<MLBRoster>> GetAllMLBRoster()
    {
        var sql = @"
            select
                convert(varchar(20), p.playerID) as PlayerId
                ,p.firstName as FirstName
                ,p.lastName as LastName
                ,p.teamCode as TeamCode
                ,coalesce(t.teamShortName, p.teamCode) as TeamName
                ,null as League
                ,convert(varchar(10), p.number) as Number
                ,coalesce(pc.positionDesc, p.positionCode) as Position
                ,mlb.throws as Throws
                ,mlb.bats as Bats
                ,mlb.battingAverage as BattingAverage
                ,mlb.homeRuns as HomeRuns
                ,mlb.era as Era
                ,convert(varchar(10), p.heightInches) as Height
                ,convert(varchar(10), p.weight) as Weight
                ,coalesce(convert(datetime, p.dateOfBirth), convert(datetime, '1900-01-01')) as DateOfBirth
                ,p.birthCountry as BirthCountry
                ,p.birthCityState as BirthCityState
            from core.Players p
            left join core.Teams t
                on t.sportCode = p.sportCode
                and t.teamCode = p.teamCode
            left join reference.PositionCodes pc
                on pc.sportCode = p.sportCode
                and pc.positionCode = p.positionCode
            left join stats.MLBPlayerStats mlb
                on mlb.sportCode = p.sportCode
                and mlb.playerID = p.playerID
            where p.sportCode = 'MLB'
            order by p.playerID, p.lastName, p.firstName";

        using (var connection = new SqlConnection(base.connectionString))
        {
            await base.GenToken(connection);
            return await connection.QueryAsync<MLBRoster>(sql);
        }

    }

    public async Task<IEnumerable<MLBRosterDto>> GetMLBRoster(ILogger<MLBRoster> logger)
    {
        var sql = @"
            select 
                convert(varchar(20), p.playerID) as PlayerId
                ,p.firstName as FirstName
                ,p.lastName as LastName
                ,p.teamCode as TeamCode
                ,coalesce(t.teamShortName, p.teamCode) as TeamName
                ,t.league as League
                ,convert(varchar(10), p.number) as Number
                ,coalesce(pc.positionDesc, p.positionCode) as Position
                ,mlb.bats as Bats
                ,mlb.throws as Throws
                ,mlb.battingAverage as BattingAverage
                ,mlb.homeRuns as HomeRuns
                ,mlb.era as Era
                ,coalesce(convert(datetime, p.dateOfBirth), convert(datetime, '1900-01-01')) as DateOfBirth
                ,convert(varchar(10), p.heightInches) as Height
                ,convert(varchar(10), p.weight) as Weight
                ,p.birthCityState as BirthCityState
                ,p.birthCountry as BirthCountry
                ,p.draftYear as DraftYear
                ,p.seasonYear as SeasonYear
            from core.Players p
            left join core.Teams t
                on t.sportCode = p.sportCode
                and t.teamCode = p.teamCode
            left join reference.PositionCodes pc
                on pc.sportCode = p.sportCode
                and pc.positionCode = p.positionCode
            left join stats.MLBPlayerStats mlb
                on mlb.sportCode = p.sportCode
                and mlb.playerID = p.playerID
            where p.sportCode = 'MLB'
            order by 
                p.playerID, p.lastName, p.firstName";
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
        var sql = @"
            select
                sportCode as teamId
                ,yearId
                ,sportCode as TeamName
                ,null as ParkName
                ,attendance
            from stats.Attendance
            where sportCode = 'MLB'
            order by yearId";

        using (var connection = new SqlConnection(base.connectionString))
        {
            await base.GenToken(connection);
            return await connection.QueryAsync<MLBAttendance>(sql);
        }
    }

    public async Task<IEnumerable<MLBAttendanceDto>> GetMLBAttendance(ILogger<MLBAttendanceDto> logger, short? year = null)
    {
        logger.LogInformation($"Fetching MLB Attendance Grid for year {year}");

        var sql = @"
        select 
            yearId
            ,sportCode as teamId
            ,sportCode as teamName
            ,null as parkName
            ,attendance
        from stats.Attendance 
        where sportCode = 'MLB'
            and (@yearId IS NULL OR yearId = @yearId)
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
                    ,sportCode as teamName
                    ,attendance
                from stats.Attendance 
                where sportCode = 'MLB'
                    and (@yearId IS NULL OR yearId = @yearId)
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
                EXEC stats.[attendanceReportSproc] @sportCode, @beginDecade, @endDecade;";
                // begin to assemble our chart payload
                mlbDecs.datasets = new List<Dataset>();
                // run the query
                await base.GenToken(connection);
                IEnumerable<MLBAttendanceDto> chartData = await connection.QueryAsync<MLBAttendanceDto>(sql, new { sportCode = "MLB", beginDecade, endDecade });
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