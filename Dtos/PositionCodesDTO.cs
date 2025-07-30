namespace AgilitySportsAPI.Dtos;

public record PositionCodesDTO
{
    public required string Sport { get; set; }
    public required string PositionCode { get; set; }
    public required string PositionDesc { get; set; }
}