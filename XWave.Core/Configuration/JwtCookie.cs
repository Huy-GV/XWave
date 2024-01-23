using System.ComponentModel.DataAnnotations;

namespace XWave.Core.Configuration;

public class JwtCookie
{
    [Required]
    public required string Name { get; init; }

    [Required] 
    public required bool HttpOnly { get; init; } = true;
    
    [Required]
    public required int DurationInDays { get; init; }
}