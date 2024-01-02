using AgilitySportsAPI.Models;
using AgilitySportsAPI.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace AgilitySportsAPI.Data;
public interface INFLRepo
{
    #region NFL

    Task<IEnumerable<NFLRoster>> GetAllNFLRoster();
    Task<IEnumerable<NFLRosterDto>> GetNFLRoster(ILogger<NFLRepo> logger);

    #endregion
}