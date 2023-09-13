using Dapper.Contrib.Extensions;

namespace AgilitySportsAPI.Models;

[Table("NHL.roster")]
public record NHLRoster
{
    [Key]
    public int? playerID { get; set; }
    public string? Name { get; set; }
    public string? Team { get; set; }
    public string? Number { get; set; }
    public string? Position { get; set; }
    public string? Handed { get; set; }
    public byte? Age { get; set; }
    public Int16? Drafted { get; set; }
    public string? BirthPlace { get; set; }
    public string? BirthCountry { get; set; }

}