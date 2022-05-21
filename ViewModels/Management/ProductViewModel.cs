using System.ComponentModel.DataAnnotations;

namespace XWave.ViewModels.Management;

public class ProductViewModel
{
    [Required] [MaxLength(30)] public string Name { get; set; }

    [Required] [MaxLength(100)] public string Description { get; set; }

    //TODO: implement image path
    //public string ImagePath { get; set; }
    public int CategoryId { get; set; }
}