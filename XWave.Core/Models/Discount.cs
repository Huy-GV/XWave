using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XWave.Core.Models;

public class Discount : IEntity
{
    public int Id { get; set; }

    [Required]
    [Range(1, 100, ErrorMessage = "Discount percentage outside valid range")]
    public uint Percentage { get; set; }

    [Required] [Column(TypeName = "date")] public DateTime StartDate { get; set; }

    [Required] [Column(TypeName = "date")] public DateTime EndDate { get; set; }

    public ICollection<Product> Products { get; set; }
}