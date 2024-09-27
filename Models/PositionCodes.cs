using Dapper.Contrib.Extensions;

namespace AgilitySportsAPI.Models;

[Table("dbo.PositionCodes")]
public record PositionCodes
{
    [Key] string Sport { get; set; }
    public string PositionCode { get; set; }
    public string PositionDesc { get; set; }
}