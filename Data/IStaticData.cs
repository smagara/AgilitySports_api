using AgilitySportsAPI.Dtos;

namespace AgilitySportsAPI.Data;
public interface IStaticData
{
    Task<IEnumerable<PositionCodesDTO>?> GetPositionCodes(ILogger<PositionCodesDTO> logger, string? sport);
}
