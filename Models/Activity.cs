using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace XWave.Models;

public enum OperationType
{
    Create,
    Modify,
    Delete
}

public class Activity : IEntity
{
    public int Id { get; set; }

    [Required]
    [Column(TypeName = "datetime")]
    public DateTime Timestamp { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 5)]
    public string EntityType { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Info { get; set; }

    public string UserId { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OperationType OperationType { get; set; }

    [JsonIgnore] public ApplicationUser AppUser { get; set; }
}