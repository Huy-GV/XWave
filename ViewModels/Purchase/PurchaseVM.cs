using XWave.Models;
using System.Collections.Generic;


namespace XWave.ViewModels.Purchase
{
    public record ProductDetail
    {
        public int ProductID { get; set; }
        public int Quantity { get; set; } 
        //TODO: remove this 
        public decimal Price { get; set; }
    }
    public class PurchaseVM
    {
        public IList<ProductDetail> ProductCart { get; set; }
        public int CustomerID { get; set; }
        public int PaymentID { get; set; }
    }
}
