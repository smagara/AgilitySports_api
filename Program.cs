using AgilitySportsAPI.Data;
var builder = WebApplication.CreateBuilder(args);
var alwaysSwagger = true;

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// add all new APIs here
builder.Services.AddScoped<INFLRepo, NFLRepo>();
builder.Services.AddScoped<INHLRepo, NHLRepo>();
builder.Services.AddScoped<INBARepo, NBARepo>();
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
all.MapGet("version", () => "0.1.0");
#endregion

#region NFL

var NFL = all.MapGroup("nfl");
NFL.MapGet("roster/all", async (INFLRepo repo) => {
    return Results.Ok(await repo.GetAllNFLRoster());
});

NFL.MapGet("roster", async (INFLRepo repo) => {
    return Results.Ok(await repo.GetNFLRoster());
});

#endregion

#region PGA
var PGA = all.MapGroup("pga");
#endregion

#region MLB
var MLB = all.MapGroup("mlb");
MLB.MapGet("roster/all", async (IMLBRepo repoBaseball) => {
    return Results.Ok(await repoBaseball.GetAllMLBRoster());
});

MLB.MapGet("roster", async (IMLBRepo repoBaseball) => {
    return Results.Ok(await repoBaseball.GetMLBRoster());
});

MLB.MapGet("attendance/all", async (IMLBRepo repoBaseball) => {
    return Results.Ok(await repoBaseball.GetAllMLBAttendance());
});

MLB.MapGet("attendance", async (IMLBRepo repoBaseball) => {
    return Results.Ok(await repoBaseball.GetMLBAttendance());
});
#endregion

#region NHL
var NHL = all.MapGroup("nhl");
NHL.MapGet("roster/all", async (INHLRepo repoNHL) => {
    return Results.Ok(await repoNHL.GetAllNHLRoster());
});

NHL.MapGet("roster", async (INHLRepo repo) => {
    return Results.Ok(await repo.GetNHLRoster());
});
#endregion

#region NBA
var NBA = all.MapGroup("nba");
NBA.MapGet("roster/all", async (INBARepo repoNBA) => {
    return Results.Ok(await repoNBA.GetAllNBARoster());
});

NBA.MapGet("roster", async (INBARepo repo) => {
    return Results.Ok(await repo.GetNBARoster());
});
#endregion

app.Run();


