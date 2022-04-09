namespace XWave.DTOs.Management
{
    public record ActivityLogDto
    {
        public int Id { get; set; }
        public string InfoText { get; set; }
        public System.DateTime Timestamp { get; set; }
    }
}