using Dapper.Contrib.Extensions;

namespace AgilitySportsAPI.Models;

[Table("NFL.roster")]
public record NFLRoster
{
    [Key]
    public int PlayerId { get; set; }
    public string? Name { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Team { get; set; }
    public string? Position { get; set; }
    public string? FantasyPosition { get; set; }
    public string? PositionCategory { get; set; }
    public string? Height { get; set; }
    public int? Weight { get; set; }
    public int? Number { get; set; }
    public string? CurrentStatus { get; set; }
    public string? CurrentStatusColor { get; set; }
    public DateTime? BirthDateShortString { get; set; }
    public string? Age { get; set; }
    public double? AgeExact { get; set; }
    public string? College { get; set; }
    public string? CollegeDraftRound { get; set; }
    public string? CollegeDraftPick { get; set; }
    public string? ExperienceDigit { get; set; }
    public string? PlayerUrlString { get; set; }
    public string? TeamName { get; set; }
    public string? TeamUrlString { get; set; }
    public string? PhotoUrl { get; set; }
    public string? PreferredHostedHeadshotUrl { get; set; }
    public string? LowResPreferredHostedHeadshotUrl { get; set; }
    public bool? IsAvailableToPlay { get; set; }
    public string? Status { get; set; }
    public string? InjuryStatus { get; set; }
    public string? InjuryBodyPart { get; set; }
    public string? ShortName { get; set; }
    public string? TeamDetails { get; set; }
    public string? CSName { get; set; }
}