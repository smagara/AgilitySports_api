namespace AgilitySportsAPI.Dtos;

public class PGARosterDto
{
    public int PlayerId { get; set; }
    public string? TeamCode { get; set; }
    public string? Team { get; set; }
    public string? TeamName { get; set; }
    public string League { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Number { get; set; }
    public string? Position { get; set; }
    public string? Height { get; set; }
    public string? Weight { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? College { get; set; }
    public string? BirthCityState { get; set; }
    public string? BirthCountry { get; set; }
    public short? DraftYear { get; set; }
    public short? SeasonYear { get; set; }
    public int? Wins { get; set; }
    public int? Majors { get; set; }
    public decimal? DrivingDistance { get; set; }
    public decimal? ScoringAverage { get; set; }
    public int? EventsPlayed { get; set; }
    public int? CutsMade { get; set; }
}
