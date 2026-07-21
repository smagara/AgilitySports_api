using System.Numerics;
using Dapper.Contrib.Extensions;

namespace AgilitySportsAPI.Models;

[Table("stats.Attendance")]
public record MLBAttendance
{
    [Key]
    public string? teamId { get; set; }
    public short? yearId { get; set; }
    public string? TeamName { get; set; }
    public string? ParkName { get; set; }    
    public long? Attendance { get; set; }

}