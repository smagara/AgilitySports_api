using AgilitySportsAPI.Models;
using AgilitySportsAPI.Dtos;

namespace AgilitySportsAPI.Data;
public interface INHLRepo
{
    #region NHL

    Task<IEnumerable<NHLRosterDto>?> GetNHLRoster(ILogger<NHLRoster> logger, int? playerId);
    Task<NHLRoster?> CreateNHLRoster(NHLRoster roster, ILogger<NHLRoster> logger);
    Task<bool> UpdateNHLRoster(NHLRoster roster, ILogger<NHLRoster> logger);
    Task<bool> DeleteNHLRoster(int playerId, ILogger<NHLRoster> logger);

    #endregion
}