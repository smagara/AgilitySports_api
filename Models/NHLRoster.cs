using Dapper.Contrib.Extensions;

namespace AgilitySportsAPI.Models;

[Table("core.Players")]
public record NHLRoster
{
    [Key]
    public int PlayerId { get; set; }
    public string? Name { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? TeamCode { get; set; }
    public string? Team { get; set; }
    public string? Number { get; set; }
    public string? Position { get; set; }
    public string? Handed { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Height { get; set; }
    public string? Weight { get; set; }
    public string? College { get; set; }
    public byte? Age { get; set; }
    public string? BirthCountry { get; set; }
    public string? BirthCityState { get; set; }
    public short? DraftYear { get; set; }
    public short? SeasonYear { get; set; }
    public int? Goals { get; set; }
    public int? PenaltyMinutes { get; set; }
    public int? Points { get; set; }
    public decimal? SavePct { get; set; }

}