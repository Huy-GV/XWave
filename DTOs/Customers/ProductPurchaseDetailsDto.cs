﻿namespace XWave.DTOs.Customers
{
    public record ProductPurchaseDetailsDto
    {
        public uint Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get => Quantity * Price; }
        public string ProductName { get; set; }
        //public string ProductImage { get; set; }
    }
}