// This file contains the endpoints for static data operations such as fetching position codes.

using AgilitySportsAPI.Data;
using AgilitySportsAPI.Dtos;

public static class StaticDataEndpoints
{
    /// <summary>
    /// Maps the static data-related endpoints such as fetching position codes.
    /// </summary>
    /// <param name="routes">The endpoint route builder.</param>
    public static void MapStaticDataEndpoints(this IEndpointRouteBuilder routes)
    {
        var staticData = routes.MapGroup("api/staticdata");
        staticData.MapGet("positions", async (
            ILogger<PositionCodesDTO> logger,
            IStaticData repoPosition,
            string? sport,
            string? sportCode) =>
        {
            var requestedSport = !string.IsNullOrWhiteSpace(sportCode) ? sportCode : sport;
            var results = await repoPosition.GetPositionCodes(logger, requestedSport);
            if (results != null)
            {
                return Results.Ok(results);
            }
            else
            {
                return Results.Problem("Error fetching sport Positions for " + requestedSport + ", ask your admin to check the logs.");
            }
        });
    }
}