using System.ComponentModel.DataAnnotations;

namespace XWave.Core.ViewModels.Management;

public class UpdateProductViewModel
{
    [Required] 
    [MaxLength(30)] 
    public string Name { get; set; } = string.Empty;

    [Required] 
    [MaxLength(100)] 
    public string Description { get; set; } = string.Empty;

    [Required] 
    public decimal Price { get; set; }

    [Required] 
    public int CategoryId { get; set; }
}