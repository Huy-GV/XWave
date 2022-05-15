using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace XWave.ViewModels.Customers
{
    public class PurchaseViewModel
    {
        public record PurchasedItems
        {
            public int ProductId { get; set; }
            public uint Quantity { get; set; }
            public decimal DisplayedPrice { get; set; }
            public decimal DisplayedDiscountPercentage { get; set; } = 0;
        }

        public IList<PurchasedItems> Cart { get; set; }
        public int PaymentAccountId { get; set; }

        [Required(ErrorMessage = "Delivery address is empty")]
        [MaxLength(100)]
        public string DeliveryAddress { get; set; }
    }
}