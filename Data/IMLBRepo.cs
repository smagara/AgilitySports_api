using AgilitySportsAPI.Models;
using AgilitySportsAPI.Dtos;

namespace AgilitySportsAPI.Data;
public interface IMLBRepo
{
    #region MLB

    Task<IEnumerable<MLBRoster>> GetAllMLBRoster();
    Task<IEnumerable<MLBRosterDto>> GetMLBRoster(ILogger<MLBRoster> logger);    
    Task<IEnumerable<MLBAttendance>> GetAllMLBAttendance();
    Task<IEnumerable<MLBAttendanceDto>> GetMLBAttendance(ILogger<MLBRoster> logger, short? year);
    Task<MLBAttendChartDTO> GetMLBChart(ILogger<MLBRoster> logger, short? year);      
     #endregion
}