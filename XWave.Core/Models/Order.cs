using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XWave.Core.Models;

public class Order : IEntity
{
    [Required] public int Id { get; set; }

    [Required]
    [Column(TypeName = "datetime")]
    public DateTime Date { get; set; } = DateTime.Now;

    public string DeliveryAddress { get; set; } = null!;

    public string CustomerId { get; set; } = null!;

    public int PaymentAccountId { get; set; }

    public CustomerAccount Customer { get; set; } = null!;

    public PaymentAccount Payment { get; set; } = null!;

    public ICollection<OrderDetails> OrderDetails { get; set; } = new List<OrderDetails>();
}