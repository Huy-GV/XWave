using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XWave.Models
{
    public class OrderDetail : IEntity
    {
        public int OrderID { get; set; }
        public int ProductID { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal PriceAtOrder { get; set; }
        public uint Quantity { get; set; }
        public Order Order { get; set; }
        public Product Product { get; set; }
    }
}
