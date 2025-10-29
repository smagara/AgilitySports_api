// -----------------------------------------------------------------------------
//  RosterExistenceService.cs
//  AgilitySportsAPI.Services
//
//  Provides a DRY, generic service for checking the existence of roster records
//  by primary key for any roster type (MLB, NBA, NFL, NHL, etc.).
//  Used by all repository classes to ensure consistent validation before updates.
//
//  Author: AgilitySports Team + Copilot
//  Date: 2025
// -----------------------------------------------------------------------------
using Microsoft.Data.SqlClient;
using Dapper.Contrib.Extensions;
using AgilitySportsAPI.Data;

namespace AgilitySportsAPI.Services;

public interface IRosterExistenceService
{
    Task<bool> ExistsAsync<T>(object key, string connectionString) where T : class;
}

public class RosterExistenceService : BaseRepo, IRosterExistenceService
{
    private readonly ILogger<RosterExistenceService> _logger;

    public RosterExistenceService(IConfiguration configuration, ILogger<RosterExistenceService> logger) : base(configuration)
    {
        _logger = logger;
    }

    public async Task<bool> ExistsAsync<T>(object key, string connectionString) where T : class
    {
        try
        {
            using var connection = new SqlConnection(connectionString);
            await GenToken(connection); // This handles Azure authentication
            await connection.OpenAsync();
            var entity = await connection.GetAsync<T>(key);
            return entity != null;
        }
        catch (Exception ex)
        {
            // Log the error but don't throw - let the calling method handle the failure
            _logger.LogError(ex, "Error checking existence for {EntityType} with key {Key}", typeof(T).Name, key);
            return false;
        }
    }
}
