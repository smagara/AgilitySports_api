
namespace AgilitySportsAPI.Dtos;

public class MLBAttendanceDto
{

    public string? TeamId { get; set; }
    public short? YearId { get; set; }
    public string? TeamName { get; set; }
    public string? ParkName { get; set; }    
    public long? Attendance { get; set; }
    
}