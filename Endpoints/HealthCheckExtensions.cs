using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;

namespace AgilitySportsAPI.Endpoints;

public static class HealthCheckExtensions
{
    public static void MapDatabaseHealthEndpoint(this IEndpointRouteBuilder routes, IConfiguration configuration)
    {
        var API = routes.MapGroup("api");
        API.MapGet("health/db", async (ILogger<object> logger) =>
        {
            string status;
            string dbMode = (configuration["Database:Mode"] ?? "").Trim();
            if (string.IsNullOrWhiteSpace(dbMode))
            {
                bool cloudOffline = bool.Parse(configuration["AzureSettings:CloudOffline"] ?? "false");
                dbMode = cloudOffline ? "LocalDb" : "Azure";
            }

            string? connStr = null;
            try
            {
                string connectionKey = dbMode.ToUpperInvariant() switch
                {
                    "AZURE" => "AzureConnection",
                    "DOCKER" => "DockerConnection",
                    "LOCALDB" => "LocalConnection",
                    _ => throw new Exception("Database:Mode must be Azure, Docker, or LocalDb.")
                };

                connStr = configuration.GetConnectionString(connectionKey);
                if (string.IsNullOrWhiteSpace(connStr))
                    throw new Exception($"Connection string '{connectionKey}' is not configured.");

                using var conn = new SqlConnection(connStr);
                await conn.OpenAsync();
                status = $"Database connection succeeded. Mode: {dbMode}, DataSource: {conn.DataSource}, DB: {conn.Database}";
                await conn.CloseAsync();
            }
            catch (Exception ex)
            {
                status = $"Database connection failed. Mode: {dbMode}, Error: {ex.Message}";
                logger.LogError(ex, "Database health check failed");
                return Results.Problem(status);
            }
            return Results.Ok(status);
        });
    }
}
