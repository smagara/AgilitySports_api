using AgilitySportsAPI.Models;
using AgilitySportsAPI.Dtos;

namespace AgilitySportsAPI.Data;
public interface INHLRepo
{
    #region NHL

    Task<IEnumerable<NHLRoster>> GetAllNHLRoster();
    Task<IEnumerable<NHLRosterDto>> GetNHLRoster();

    #endregion
}