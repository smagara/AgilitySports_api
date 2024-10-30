using AgilitySportsAPI.Models;
using AgilitySportsAPI.Dtos;

namespace AgilitySportsAPI.Data;
public interface INBARepo
{
    #region NBA
    Task<IEnumerable<NBARosterDto>?> GetNBARoster(ILogger<NBARoster> logger, int? playerId);
    Task<NBARoster?> CreateNBARoster(NBARoster roster, ILogger<NBARoster> logger);
    Task<bool> UpdateNBARoster(NBARoster roster, ILogger<NBARoster> logger);
    Task<bool> DeleteNBARoster(int playerId, ILogger<NBARoster> logger);

    #endregion
}