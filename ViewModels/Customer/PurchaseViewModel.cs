using XWave.Models;
using System.Collections.Generic;


namespace XWave.ViewModels.Customers
{

    public class PurchaseViewModel
    {
        public record ProductDetail
        {
            public int ProductId { get; set; }
            public uint Quantity { get; set; }
            public decimal DisplayedPrice { get; set; }
            public decimal DisplayedDiscount { get; set; } = 0;
        }
        public IList<ProductDetail> ProductCart { get; set; }
        public int PaymentAccountId { get; set; }
    }
}
