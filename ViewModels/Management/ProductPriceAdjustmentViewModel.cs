using System.ComponentModel.DataAnnotations;

namespace XWave.ViewModels.Management
{
    public class ProductPriceAdjustmentViewModel
    {
        [Required]
        public uint UpdatedPrice { get; set; }

        public System.DateTime? Schedule { get; set; } = null;
    }
}