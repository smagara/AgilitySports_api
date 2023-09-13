namespace AgilitySportsAPI.Dtos;

public class NBARosterDto
{

    public int? playerID { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Team { get; set; }
    public string? Number { get; set; }
    public string? Position { get; set; }
    public string? Height { get; set; }
    public string? Weight { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? College { get; set; }
}