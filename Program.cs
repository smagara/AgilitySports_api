using AgilitySportsAPI.Data;
using AgilitySportsAPI.Dtos;
using AgilitySportsAPI.Models;
using AgilitySportsAPI.Utilities;
var builder = WebApplication.CreateBuilder(args);
var alwaysSwagger = true;
const short defaultChartYear = 2019;
const short defaultDecadesBegin = 1920;
const short defaultDecadesEnd = 2010;

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add support for Log4Net logging
//builder.Logging.ClearProviders();
ILoggingBuilder logB = builder.Logging.AddLog4Net(
    new Log4NetProviderOptions()
    {
        Log4NetConfigFileName = "log4net.config"
    }
    );


// add all new APIs here
builder.Services.AddScoped<INFLRepo, NFLRepo>();
builder.Services.AddScoped<INHLRepo, NHLRepo>();
builder.Services.AddScoped<INBARepo, NBARepo>();
builder.Services.AddTransient<IColorWheel, ColorWheel>();  // for MLB Colorwheel DI
builder.Services.AddScoped<IMLBRepo, MLBRepo>();
builder.Services.AddScoped<IStaticData, StaticData>();

builder.Services.AddCors();  // CORS Error: XMLHttpRequest. has been blocked by CORS policy: No 'Access-Control-Allow-Origin'
var app = builder.Build();

app.UseCors(builder => builder // CORS remedy
.AllowAnyOrigin()
.AllowAnyMethod()
.AllowAnyHeader()
);

// Configure the HTTP request pipeline.
if (alwaysSwagger || app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


#region Version
var all = app.MapGroup("api");
all.MapGet("version", () => "1.1.0");
#endregion

#region NFL

var NFL = all.MapGroup("nfl");
NFL.MapGet("roster/all", async (INFLRepo repo) => {
    return Results.Ok(await repo.GetAllNFLRoster());
});

NFL.MapGet("roster", async (INFLRepo repo, ILogger<NFLRepo> logger) => {
    return Results.Ok(await repo.GetNFLRoster(logger));
});

#endregion

#region PGA
var PGA = all.MapGroup("pga");
#endregion

#region MLB
var MLB = all.MapGroup("mlb");
MLB.MapGet("roster/all", async (ILogger<MLBRoster> logger, IMLBRepo repoBaseball) => {
    return Results.Ok(await repoBaseball.GetAllMLBRoster());
});

MLB.MapGet("roster", async (ILogger<MLBRoster> logger, IMLBRepo repoBaseball) => {
    return Results.Ok(await repoBaseball.GetMLBRoster(logger));
});

MLB.MapGet("attendance/all", async (ILogger<MLBRoster> logger, IMLBRepo repoBaseball) => {
    return Results.Ok(await repoBaseball.GetAllMLBAttendance());
});

// optional Year parameter
MLB.MapGet("attendance", async (ILogger<MLBAttendanceDto> logger, short? yearId, IMLBRepo repoBaseball) => {
    return Results.Ok(await repoBaseball.GetMLBAttendance(logger, yearId));
});

MLB.MapGet("chart", async (ILogger<MLBAttendChartDTO> logger, short? yearId, IMLBRepo repoBaseball) => {
    return Results.Ok(await repoBaseball.GetMLBChart(logger, yearId ?? defaultChartYear));
});

MLB.MapGet("decades", async (ILogger<MLBAttendChartDTO> logger, short? beginDecade, short? endDecade, IMLBRepo repoBaseball) => {
    return Results.Ok(await repoBaseball.GetMLBDecades(logger, beginDecade ?? defaultDecadesBegin, endDecade ?? defaultDecadesEnd));
});
#endregion

#region NHL
var NHL = all.MapGroup("nhl");

// Read
NHL.MapGet("roster", async (ILogger<NHLRoster> logger, int? playerId, INHLRepo repo) =>
{
    var results = await repo.GetNHLRoster(logger, playerId);
    if (results != null)
    {
        return Results.Ok(results);
    }
    else
    {
        return Results.Problem("Error fetching NHL Roster, ask your admin to check the logs.");
    }   
});

// Create
NHL.MapPost("roster", async (ILogger<NHLRoster> logger, INHLRepo repo, NHLRoster roster) =>
{
    NHLRoster? newPlayer = await repo.CreateNHLRoster(roster, logger);

    if (newPlayer != null)
    {
        return Results.Ok("Added to NHL Roster.");
    }
    else
    {
        return Results.Problem("Error adding to NHL Roster, check the logs.");
    }
});

// Update
NHL.MapPut("roster", async (ILogger<NHLRoster> logger, INHLRepo repo, NHLRoster roster) =>
{
    bool ret = await repo.UpdateNHLRoster(roster, logger);

    if (ret == true)
    {
        return Results.Ok("Updated NHL Roster.");
    }
    else
    {
        return Results.Problem("Error updating the NHL Roster, check the logs.");
    }
});

// Delete
NHL.MapDelete("roster", async (ILogger<NHLRoster> logger, INHLRepo repo, int playerId) =>
{

    bool ret = await repo.DeleteNHLRoster(playerId, logger);

    if (ret == true)
    {
        return Results.Ok("Deleted from NHL Roster.");
    }
    else
    {
        return Results.Problem("Error deleting from NHL Roster, check the logs.");

    }
});
#endregion

#region NBA
var NBA = all.MapGroup("nba");

// Read
NBA.MapGet("roster", async (ILogger<NBARoster> logger, int? playerId, INBARepo repo) =>
{
    var results = await repo.GetNBARoster(logger, playerId);
    if (results != null)
    {
        return Results.Ok(results);
    }
    else
    {
        return Results.Problem("Error fetching NBA Roster, ask your admin to check the logs.");
    }   
});

// Create
NBA.MapPost("roster", async (ILogger<NBARoster> logger, INBARepo repo, NBARoster roster) =>
{
    NBARoster? newPlayer = await repo.CreateNBARoster(roster, logger);

    if (newPlayer != null)
    {
        return Results.Ok("Added to NBA Roster.");
    }
    else
    {
        return Results.Problem("Error adding to NBA Roster, check the logs.");
    }
});

// Update
NBA.MapPut("roster", async (ILogger<NBARoster> logger, INBARepo repo, NBARoster roster) =>
{
    bool ret = await repo.UpdateNBARoster(roster, logger);

    if (ret == true)
    {
        return Results.Ok("Updated NBA Roster.");
    }
    else
    {
        return Results.Problem("Error updating the NBA Roster, check the logs.");
    }
});

// Delete
NBA.MapDelete("roster", async (ILogger<NBARoster> logger, INBARepo repo, int playerId) =>
{

    bool ret = await repo.DeleteNBARoster(playerId, logger);

    if (ret == true)
    {
        return Results.Ok("Deleted from NBA Roster.");
    }
    else
    {
        return Results.Problem("Error deleting from NBA Roster, check the logs.");

    }
});
#endregion

#region StaticData
var staticData = all.MapGroup("staticdata");
staticData.MapGet("positions", async (ILogger<PositionCodes> logger, IStaticData repoPosition, string sport) =>
{
    var results = await repoPosition.GetPositionCodes(logger, sport);
    if (results != null)
    {
        return Results.Ok(results);
    }
    else
    {
        return Results.Problem("Error fetching sport Positions for " + sport + ", ask your admin to check the logs.");
    } 
});

#endregion

app.Run();


