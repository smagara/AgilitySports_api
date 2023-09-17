namespace AgilitySportsAPI.Dtos;

public class MLBRosterDto
{

    public string? playerID { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? TeamName { get; set; }    
    public string? League { get; set; }
    public string? Number { get; set; }
    public string? Position { get; set; }
    public string? Throws { get; set; }    
    public string? Bats { get; set; }
    public string? Height { get; set; }    
    public string? Weight { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? BirthCountry { get; set; }
    public string? BirthPlace { get; set; }
}