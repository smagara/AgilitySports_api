using AgilitySportsAPI.Models;
using AgilitySportsAPI.Dtos;

namespace AgilitySportsAPI.Data;
public interface IMLBRepo
{
    #region MLB

    Task<IEnumerable<MLBRoster>> GetAllMLBRoster();
    Task<IEnumerable<MLBRosterDto>> GetMLBRoster();    
    Task<IEnumerable<MLBAttendance>> GetAllMLBAttendance();
    Task<IEnumerable<MLBAttendanceDto>> GetMLBAttendance(short? year);
    Task<MLBAttendChartDTO> GetMLBChart(short? year);      
    Task<MLBAttendChartDTO> GetMLBDecades(short? beginDecade, short? endDecade);    
    #endregion
}