namespace AgilitySportsAPI.Dtos;

public class PlayerSummaryDto
{
    public int PlayerID { get; set; }
    public string SportCode { get; set; } = null!;
    public string TeamCode { get; set; } = null!;
    public string? TeamName { get; set; }
    public string? PositionCode { get; set; }
    public string? PositionDesc { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int? JerseyNumber { get; set; }
    public short? SeasonYear { get; set; }
}
