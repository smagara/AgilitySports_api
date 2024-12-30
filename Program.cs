using AgilitySportsAPI.Data;
using AgilitySportsAPI.Dtos;
using AgilitySportsAPI.Models;
using AgilitySportsAPI.Utilities;
var builder = WebApplication.CreateBuilder(args);
var alwaysSwagger = true;

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add support for Log4Net logging
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

// Map REST API endpoints using extension methods
app.MapSystemEndpoints(app.Configuration);
app.MapNbaEndpoints();
app.MapNflEndpoints();
app.MapMlbEndpoints();
app.MapNhlEndpoints();
app.MapStaticDataEndpoints();
app.Run();


