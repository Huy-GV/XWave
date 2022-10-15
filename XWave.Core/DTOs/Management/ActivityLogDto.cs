namespace XWave.Core.DTOs.Management;

public record ActivityLogDto
{
    public int Id { get; init; }
    public string InfoText { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}