namespace AgilitySportsAPI.Dtos;

public class NFLRosterDto
{
    public int playerID { get; set; }
    public string TeamCode { get; set; } = null!;
    public string Team { get; set; } = null!;
    public string TeamName { get; set; } = null!;
    public string League { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Position { get; set; } = null!;
    public string Number { get; set; } = null!;
    public string Height { get; set; } = null!;
    public string Weight { get; set; } = null!;
    public DateTime? DateOfBirth { get; set; }
    public int? Age { get; set; }
    public string College { get; set; } = null!;
    public short? YearDrafted { get; set; }
}