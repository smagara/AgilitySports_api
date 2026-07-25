namespace AgilitySportsAPI.Dtos;

public class PlayerStatsDto
{
    public int PlayerID { get; set; }
    public string SportCode { get; set; } = null!;

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
    public int? Sacks { get; set; }
    public int? Touchdowns { get; set; }

    // NHL
    public string? Handed { get; set; }
    public int? Goals { get; set; }
    public int? PenaltyMinutes { get; set; }
    public int? Points { get; set; }
    public decimal? SavePct { get; set; }
}
