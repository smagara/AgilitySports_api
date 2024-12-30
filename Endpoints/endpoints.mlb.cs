// This file contains the endpoints for MLB-related operations such as fetching rosters, attendance, and chart data.

using AgilitySportsAPI.Data;
using AgilitySportsAPI.Dtos;
using AgilitySportsAPI.Models;

public static class MlbDataEndpoints
{
    const short defaultChartYear = 2019;
    const short defaultDecadesBegin = 1920;
    const short defaultDecadesEnd = 2010;

    /// <summary>
    /// Maps the MLB-related endpoints such as fetching rosters, attendance, and chart data.
    /// </summary>
    /// <param name="routes">The endpoint route builder.</param>
    public static void MapMlbEndpoints(this IEndpointRouteBuilder routes)
    {
        var MLB = routes.MapGroup("api/mlb");

        // Endpoint to get all MLB rosters
        MLB.MapGet("roster/all", async (ILogger<MLBRoster> logger, IMLBRepo repoBaseball) =>
        {
            return Results.Ok(await repoBaseball.GetAllMLBRoster());
        });

        // Endpoint to get MLB roster with logging
        MLB.MapGet("roster", async (ILogger<MLBRoster> logger, IMLBRepo repoBaseball) =>
        {
            return Results.Ok(await repoBaseball.GetMLBRoster(logger));
        });

        // Endpoint to get all MLB attendance records
        MLB.MapGet("attendance/all", async (ILogger<MLBRoster> logger, IMLBRepo repoBaseball) =>
        {
            return Results.Ok(await repoBaseball.GetAllMLBAttendance());
        });

        // Endpoint to get MLB attendance records with optional year parameter
        MLB.MapGet("attendance", async (ILogger<MLBAttendanceDto> logger, short? yearId, IMLBRepo repoBaseball) =>
        {
            return Results.Ok(await repoBaseball.GetMLBAttendance(logger, yearId));
        });

        // Endpoint to get MLB chart data with optional year parameter
        MLB.MapGet("chart", async (ILogger<MLBAttendChartDTO> logger, short? yearId, IMLBRepo repoBaseball) =>
        {
            return Results.Ok(await repoBaseball.GetMLBChart(logger, yearId ?? defaultChartYear));
        });

        // Endpoint to get MLB decades data with optional begin and end decade parameters
        MLB.MapGet("decades", async (ILogger<MLBAttendChartDTO> logger, short? beginDecade, short? endDecade, IMLBRepo repoBaseball) =>
        {
            return Results.Ok(await repoBaseball.GetMLBDecades(logger, beginDecade ?? defaultDecadesBegin, endDecade ?? defaultDecadesEnd));
        });
    }
}