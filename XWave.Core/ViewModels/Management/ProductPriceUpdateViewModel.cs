using System.ComponentModel.DataAnnotations;

namespace XWave.Core.ViewModels.Management;

public class ProductPriceUpdateViewModel
{
    [Required] 
    public uint UpdatedPrice { get; set; }

    public DateTime? Schedule { get; set; } = null;
}