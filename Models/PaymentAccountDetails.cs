using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace XWave.Models
{
    public class PaymentAccountDetails : IEntity
    {
        public string CustomerId { get; set; }
        public int PaymentAccountId { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime FirstRegistration { get; set; }

        public CustomerAccount Customer { get; set; }
        public PaymentAccount Payment { get; set; }
    }
}