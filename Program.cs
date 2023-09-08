using AgilitySportsAPI.Data;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<INFLRepo, NFLRepo>();

builder.Services.AddCors();  // CORS Error: XMLHttpRequest. has been blocked by CORS policy: No 'Access-Control-Allow-Origin'
var app = builder.Build();

app.UseCors(builder => builder // CORS remedy
.AllowAnyOrigin()
.AllowAnyMethod()
.AllowAnyHeader()
);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
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
var PGA = all.MapGroup("pga");
NFL.MapGet("roster/all", async (INFLRepo repo) => {
    return Results.Ok(await repo.GetAllNFLRoster());
});

NFL.MapGet("roster", async (INFLRepo repo) => {
    return Results.Ok(await repo.GetNFLRoster());
});

#endregion

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
