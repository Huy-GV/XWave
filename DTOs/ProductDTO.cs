﻿using System;

namespace XWave.DTOs
{
    //TODO: remove all methods from DTO and use a mapper library
    public class ProductDTO
    {
        public int ID { get; init; }
        public string Name { get; init; }
        public string CategoryName { get; init; }
        public decimal Price { get; init; }
        public uint Quantity { get; init; }
        public int CategoryID { get; init; }
        public DiscountDTO? Discount { get; init; }
    }
}
