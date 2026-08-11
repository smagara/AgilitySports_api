using AgilitySportsAPI.Models;
using AgilitySportsAPI.Dtos;

namespace AgilitySportsAPI.Data;

public interface IPGARepo
{
    Task<IEnumerable<PGARosterDto>?> GetPGARoster(ILogger<PGARoster> logger, int? playerId);
    Task<PGARoster?> CreatePGARoster(PGARoster roster, ILogger<PGARoster> logger);
    Task<bool> UpdatePGARoster(PGARoster roster, ILogger<PGARoster> logger);
    Task<bool> DeletePGARoster(int playerId, ILogger<PGARoster> logger);
}
