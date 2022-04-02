using System;
using System.ComponentModel.DataAnnotations;

namespace XWave.Models
{
    public class PaymentAccount : IEntity
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; }
        public string Provider { get; set; }

        [DataType(DataType.Date)]
        public DateTime ExpiryDate { get; set; }
    }
}