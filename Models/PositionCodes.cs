using Dapper.Contrib.Extensions;

namespace AgilitySportsAPI.Models;

[Table("dbo.PositionCodes")]
public record PositionCodes
{
    [Key] public required string Sport { get; set; }
    public required string PositionCode { get; set; }
    public required string PositionDesc { get; set; }
}