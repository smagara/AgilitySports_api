using AgilitySportsAPI.Models;
using AgilitySportsAPI.Dtos;

namespace AgilitySportsAPI.Data;
public interface IStaticData
{
    Task<IEnumerable<PositionCodes>?> GetPositionCodes(ILogger<PositionCodes> logger, string sport);
}
