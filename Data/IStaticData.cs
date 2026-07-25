using AgilitySportsAPI.Dtos;

namespace AgilitySportsAPI.Data;
public interface IStaticData
{
    Task<IEnumerable<PositionCodesDTO>?> GetPositionCodes(ILogger<PositionCodesDTO> logger, string? sport);
    Task<IEnumerable<TeamLeagueDto>?> GetTeamLeagues(ILogger<TeamLeagueDto> logger, string? sport);
}
