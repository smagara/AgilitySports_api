using AgilitySportsAPI.Models;
using AgilitySportsAPI.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace AgilitySportsAPI.Data;
public interface INFLRepo
{
    #region NFL

    Task<IEnumerable<NFLRosterDto>?> GetNFLRoster(ILogger<NFLRoster> logger, int? playerId);

    #endregion
}