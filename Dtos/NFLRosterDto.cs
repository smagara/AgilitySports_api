namespace AgilitySportsAPI.Dtos;

public class NFLRosterDto
{
    public int playerID { get; set; }
    public string team { get; set; } = null!;
    public string name { get; set; } = null!;
    public string position { get; set; } = null!;
    public string number { get; set; } = null!;
    public string height { get; set; } = null!;
    public string weight { get; set; } = null!;
    public double ageExact { get; set; }
    public string college { get; set; } = null!;
}