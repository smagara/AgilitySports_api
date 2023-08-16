using AgilitySportsAPI.Models;

namespace AgilitySportsAPI.Data;
public interface INFLRepo
{
    #region NFL

    Task<IEnumerable<NFLRoster>> GetAllNFLRoster();

    #endregion
}