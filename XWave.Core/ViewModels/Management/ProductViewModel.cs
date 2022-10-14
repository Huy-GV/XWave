using System.ComponentModel.DataAnnotations;

namespace XWave.Core.ViewModels.Management;

public class ProductViewModel
{
    [Required] 
    [MaxLength(30)] public string Name { get; set; } = string.Empty;

    [Required] 
    [MaxLength(100)] public string Description { get; set; } = string.Empty;

    //TODO: implement image path
    //public string ImagePath { get; set; }

    [Required] 
    public int CategoryId { get; set; }
}