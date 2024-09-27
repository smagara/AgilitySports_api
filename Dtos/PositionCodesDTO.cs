namespace AgilitySportsAPI.Dtos;

public record PositionCodesDTO
{
    string Sport { get; set; }
    public string PositionCode { get; set; }
    public string PositionDesc { get; set; }
}