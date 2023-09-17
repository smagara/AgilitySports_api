using AgilitySportsAPI.Models;
using AgilitySportsAPI.Dtos;

namespace AgilitySportsAPI.Data;
public interface IMLBRepo
{
    #region MLB

    Task<IEnumerable<MLBRoster>> GetAllMLBRoster();
    Task<IEnumerable<MLBRosterDto>> GetMLBRoster();

    #endregion
}