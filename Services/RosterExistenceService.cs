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

namespace AgilitySportsAPI.Services;

public interface IRosterExistenceService
{
    Task<bool> ExistsAsync<T>(object key, string connectionString) where T : class;
}

public class RosterExistenceService : IRosterExistenceService
{
    public async Task<bool> ExistsAsync<T>(object key, string connectionString) where T : class
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        var entity = await connection.GetAsync<T>(key);
        return entity != null;
    }
}
