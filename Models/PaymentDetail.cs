using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XWave.Models
{
    public class PaymentDetail
    {
        public string CustomerID { get; set; }
        public int PaymentID { get; set; }
        [DataType(DataType.Date)]
        public DateTime Registration { get; set; }
        [Range(0, int.MaxValue)]
        public uint PurchaseCount { get; set; }
        [DataType(DataType.Date)]
        public DateTime? LatestPurchase { get; set; }
        //navigation property
        public Customer Customer { get; set; }
        public Payment Payment { get; set; }
    }
}
