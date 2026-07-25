namespace AgilitySportsAPI.Dtos;

public class PlayerUpsertDto
{
    public string SportCode { get; set; } = null!;
    public string TeamCode { get; set; } = null!;
    public string? PositionCode { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Height { get; set; }
    public int? Weight { get; set; }
    public int? Number { get; set; }
    public string? College { get; set; }
    public string? BirthCityState { get; set; }
    public string? BirthCountry { get; set; }
    public short? DraftYear { get; set; }
    public short? SeasonYear { get; set; }
}
