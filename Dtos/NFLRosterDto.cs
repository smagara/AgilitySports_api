namespace AgilitySportsAPI.Dtos;

public class NFLRosterDto
{
    public int playerID { get; set; }
    public string team { get; set; } = null!;
    public string firstName { get; set; } = null!;
    public string lastName { get; set; } = null!;
    public string position { get; set; } = null!;
    public string number { get; set; } = null!;
    public string height { get; set; } = null!;
    public string weight { get; set; } = null!;
    public int age { get; set; }
    public string college { get; set; } = null!;
}