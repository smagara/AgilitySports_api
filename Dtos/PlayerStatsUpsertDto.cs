namespace AgilitySportsAPI.Dtos;

public class PlayerStatsUpsertDto
{
    // MLB
    public string? Bats { get; set; }
    public string? Throws { get; set; }
    public decimal? BattingAverage { get; set; }
    public int? HomeRuns { get; set; }
    public decimal? Era { get; set; }

    // NBA
    public decimal? PointsPerGame { get; set; }
    public decimal? ReboundsPerGame { get; set; }
    public decimal? AssistsPerGame { get; set; }

    // NFL
    public decimal? Sacks { get; set; }
    public int? Touchdowns { get; set; }

    // NHL
    public string? Handed { get; set; }
    public int? Goals { get; set; }
    public int? PenaltyMinutes { get; set; }
    public int? Points { get; set; }
    public decimal? SavePct { get; set; }

    // FIF (FIFA World Cup)
    public int? TotalGoals { get; set; }
    public int? Assists { get; set; }
    public int? Saves { get; set; }

    // PGA
    public int? Wins { get; set; }
    public int? Majors { get; set; }
    public decimal? DrivingDistance { get; set; }
    public decimal? ScoringAverage { get; set; }
    public int? EventsPlayed { get; set; }
    public int? CutsMade { get; set; }
}
