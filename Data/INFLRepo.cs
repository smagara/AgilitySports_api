using AgilitySportsAPI.Models;
using AgilitySportsAPI.Dtos;

namespace AgilitySportsAPI.Data;
public interface INFLRepo
{
    #region NFL

    Task<IEnumerable<NFLRosterDto>?> GetNFLRoster(ILogger<NFLRoster> logger, int? playerId);
    Task<NFLRoster?> Create(NFLRoster player, ILogger<NFLRoster> logger);
    Task<bool> Update(NFLRoster player, ILogger<NFLRoster> logger);
    Task<bool> Delete(int playerId, ILogger<NFLRoster> logger);

    #endregion
}