using AgilitySportsAPI.Models;
using AgilitySportsAPI.Dtos;

namespace AgilitySportsAPI.Data;
public interface INBARepo
{
    #region NHL
    Task<IEnumerable<NBARoster>> GetAllNBARoster();
    Task<IEnumerable<NBARosterDto>> GetNBARoster(ILogger<NBARoster> logger);

    #endregion
}