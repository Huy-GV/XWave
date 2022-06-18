using System.ComponentModel.DataAnnotations;

namespace XWave.Core.ViewModels.Customers;

public class PurchaseViewModel
{
    public IList<PurchasedItems> Cart { get; set; }
    public int PaymentAccountId { get; set; }

    [Required(ErrorMessage = "Delivery address is empty")]
    [MaxLength(100)]
    public string DeliveryAddress { get; set; }

    public record PurchasedItems
    {
        public int ProductId { get; set; }
        public uint Quantity { get; set; }
        public decimal DisplayedPrice { get; set; }
        public decimal DisplayedDiscountPercentage { get; set; } = 0;
    }
}