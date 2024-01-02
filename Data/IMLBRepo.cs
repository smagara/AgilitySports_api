using AgilitySportsAPI.Models;
using AgilitySportsAPI.Dtos;

namespace AgilitySportsAPI.Data;
public interface IMLBRepo
{
    #region MLB

    Task<IEnumerable<MLBRoster>> GetAllMLBRoster();
    Task<IEnumerable<MLBRosterDto>> GetMLBRoster(ILogger<MLBRoster> logger);    
    Task<IEnumerable<MLBAttendance>> GetAllMLBAttendance();
    Task<IEnumerable<MLBAttendanceDto>> GetMLBAttendance(ILogger<MLBAttendanceDto> logger, short? year);
    Task<MLBAttendChartDTO> GetMLBChart(ILogger<MLBAttendChartDTO> logger, short? year); 
    Task<MLBAttendChartDTO> GetMLBDecades(ILogger<MLBAttendChartDTO> logger, short? beginDecade, short? endDecade);   
     #endregion
}