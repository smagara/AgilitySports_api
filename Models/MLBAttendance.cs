using System.Numerics;
using Dapper.Contrib.Extensions;

namespace AgilitySportsAPI.Models;

[Table("MLB.attendance")]
public record MLBAttendance
{
    [Key]
    public string? teamId { get; set; }
    public Int16? yearId { get; set; }
    public string? TeamName { get; set; }
    public string? ParkName { get; set; }    
    public long? Attendance { get; set; }

}