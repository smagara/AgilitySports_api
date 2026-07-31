using AgilitySportsAPI.Models;
using AgilitySportsAPI.Dtos;

namespace AgilitySportsAPI.Data;

public interface IFIFRepo
{
    Task<IEnumerable<FIFRosterDto>?> GetFIFRoster(ILogger<FIFRoster> logger, int? playerId);
    Task<FIFRoster?> CreateFIFRoster(FIFRoster roster, ILogger<FIFRoster> logger);
    Task<bool> UpdateFIFRoster(FIFRoster roster, ILogger<FIFRoster> logger);
    Task<bool> DeleteFIFRoster(int playerId, ILogger<FIFRoster> logger);
}
