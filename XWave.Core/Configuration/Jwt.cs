using System.ComponentModel.DataAnnotations;

namespace XWave.Core.Configuration;

public class Jwt
{
    [Required]
    [MinLength(32)]
    public required string Key { get; init; } 
    
    [Required]
    public required string  Issuer { get; init; } 
    
    [Required]
    public required string Audience { get; init; }

    [Required]
    public required double DurationInMinutes { get; init; }
}