using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

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
