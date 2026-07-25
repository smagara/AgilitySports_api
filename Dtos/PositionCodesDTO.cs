namespace AgilitySportsAPI.Dtos;

public record PositionCodesDTO
{
    public required string SportCode { get; set; }
    public string Sport => SportCode;
    public required string PositionCode { get; set; }
    public required string PositionDesc { get; set; }
}