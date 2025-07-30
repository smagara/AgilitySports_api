// This file contains the endpoints for static data operations such as fetching position codes.

using AgilitySportsAPI.Data;
using AgilitySportsAPI.Models;

public static class StaticDataEndpoints
{
    /// <summary>
    /// Maps the static data-related endpoints such as fetching position codes.
    /// </summary>
    /// <param name="routes">The endpoint route builder.</param>
    public static void MapStaticDataEndpoints(this IEndpointRouteBuilder routes)
    {
        try
        {
            var staticData = routes.MapGroup("api/staticdata");
            staticData.MapGet("positions", async (ILogger<PositionCodes> logger, IStaticData repoPosition, string sport) =>
            {
                try
                {
                    int i = 1;
                    i++;
                }
                catch (Exception ex)
                {
                    // flag this one as well
                }
                var results = await repoPosition.GetPositionCodes(logger, sport);
                if (results != null)
                {
                    return Results.Ok(results);
                }
                else
                {
                    return Results.Problem("Error fetching sport Positions for " + sport + ", ask your admin to check the logs.");
                }
            });
        }
        catch
        {
            // this exception doesn't log or rethrow.  The GitHub pipeline should fail if this happens.
        }
    }
}