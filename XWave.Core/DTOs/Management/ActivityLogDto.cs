namespace XWave.Core.DTOs.Management;

public record ActivityLogDto
{
    public int Id { get; set; }
    public string InfoText { get; set; }
    public DateTime Timestamp { get; set; }
}