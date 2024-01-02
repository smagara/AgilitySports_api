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
all.MapGet("version", () => "0.2.0");
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
NHL.MapGet("roster/all", async (ILogger<NHLRoster> logger, INHLRepo repoNHL) => {
    return Results.Ok(await repoNHL.GetAllNHLRoster());
});

NHL.MapGet("roster", async (ILogger<NHLRoster> logger, INHLRepo repo) => {
    return Results.Ok(await repo.GetNHLRoster(logger));
});
#endregion

#region NBA
var NBA = all.MapGroup("nba");
NBA.MapGet("roster/all", async (INBARepo repoNBA) => {
    return Results.Ok(await repoNBA.GetAllNBARoster());
});
NBA.MapGet("roster", async (ILogger<NBARoster> logger, INBARepo repo) => {
    return Results.Ok(await repo.GetNBARoster(logger));
});
#endregion

app.Run();


