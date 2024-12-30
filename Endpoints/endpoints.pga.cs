// This file contains the endpoints for PGA-related operations.

using AgilitySportsAPI.Data;
using AgilitySportsAPI.Models;

public static class PgaEndpoints
{
    /// <summary>
    /// Maps the PGA-related endpoints.
    /// </summary>
    /// <param name="routes">The endpoint route builder.</param>
    public static void MapPgaEndpoints(this IEndpointRouteBuilder routes)
    {
        var PGA = routes.MapGroup("api/pga");

        // Mapping endpoints will go here
    }
}