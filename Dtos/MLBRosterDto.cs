namespace AgilitySportsAPI.Dtos;

public class MLBRosterDto
{

    public string? PlayerId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? TeamCode { get; set; }
    public string? TeamName { get; set; }    
    public string? League { get; set; }
    public string? Number { get; set; }
    public string? Position { get; set; }
    public string? Throws { get; set; }    
    public string? Bats { get; set; }
    public decimal? BattingAverage { get; set; }
    public int? HomeRuns { get; set; }
    public decimal? Era { get; set; }
    public string? Height { get; set; }    
    public string? Weight { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? BirthCountry { get; set; }
    public string? BirthCityState { get; set; }
    public short? DraftYear { get; set; }
    public short? SeasonYear { get; set; }
}