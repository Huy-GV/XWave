using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace XWave.Models
{
    public enum OperationType
    { 
        Create,
        Modify,
        Delete
    }
    public class ActivityLog : IEntity
    {
        public int Id { get; set; }
        [Required]
        [Column(TypeName ="datetime")]
        public DateTime Time { get; set; }
        [Required]
        [StringLength(200, MinimumLength =5)]
        public string EntityType { get; set; }
        public string StaffId { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OperationType OperationType { get; set; }
        public ApplicationUser StaffUser { get; set; }
    }
}
