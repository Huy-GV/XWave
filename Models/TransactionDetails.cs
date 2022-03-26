using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace XWave.Models
{
    public enum TransactionType
    {
        Purchase,
        Refund,
        Salary,
        PaymentAccountRegistration
    }

    public class TransactionDetails : IEntity
    {
        public string CustomerId { get; set; }
        public int PaymentAccountId { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime Registration { get; set; }

        [Range(0, int.MaxValue)]
        public uint PurchaseCount { get; set; }

        [DataType(DataType.Date)]
        public DateTime? LatestPurchase { get; set; } = null;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TransactionType TransactionType { get; set; }

        public CustomerAccount Customer { get; set; }
        public PaymentAccount Payment { get; set; }
    }
}