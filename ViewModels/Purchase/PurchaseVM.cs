using XWave.Models;
using System.Collections.Generic;


namespace XWave.ViewModels.Purchase
{
    public record ProductDetail
    {
        public int ProductID { get; set; }
        public uint Quantity { get; set; } 
        public decimal DisplayedPrice { get; set; }
        public decimal DisplayedDiscount { get; set; } = 0;
    }
    public class PurchaseVM
    {
        public IList<ProductDetail> ProductCart { get; set; }
        //public string CustomerID { get; set; }
        public int PaymentID { get; set; }
    }
}
