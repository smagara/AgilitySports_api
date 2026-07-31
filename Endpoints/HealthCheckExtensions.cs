using AgilitySportsAPI.Data;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;

namespace AgilitySportsAPI.Endpoints;

public static class HealthCheckExtensions
{
    public static void MapDatabaseHealthEndpoint(this IEndpointRouteBuilder routes, IConfiguration configuration)
    {
        var v2Api = routes.MapGroup("api/v2");
        v2Api.MapGet("health/db", async (ILogger<object> logger) =>
        {
            try
            {
                var probe = new DbHealthProbeRepo(configuration);
                using var conn = new SqlConnection(probe.ConnectionString);
                await probe.ApplyAuthTokenAsync(conn);
                await conn.OpenAsync();
                string status = $"Database connection succeeded. Mode: {probe.DatabaseMode}, DataSource: {conn.DataSource}, DB: {conn.Database}";
                await conn.CloseAsync();
                return Results.Ok(status);
            }
            catch (Exception ex)
            {
                string dbMode = ResolveDatabaseMode(configuration);
                string status = $"Database connection failed. Mode: {dbMode}, Error: {ex.Message}";
                logger.LogError(ex, "Database health check failed");
                return Results.Problem(status);
            }
        });
    }

    private static string ResolveDatabaseMode(IConfiguration configuration)
    {
        string? configuredMode = configuration["Database:Mode"];
        if (!string.IsNullOrWhiteSpace(configuredMode))
        {
            return configuredMode.Trim();
        }

        bool cloudOffline = bool.Parse(configuration["AzureSettings:CloudOffline"] ?? "false");
        return cloudOffline ? "LocalDb" : "Azure";
    }

    private sealed class DbHealthProbeRepo : BaseRepo
    {
        public DbHealthProbeRepo(IConfiguration configuration) : base(configuration)
        {
        }

        public string ConnectionString => connectionString;

        public string DatabaseMode => databaseMode;

        public Task ApplyAuthTokenAsync(SqlConnection connection) => GenToken(connection);
    }
}
