﻿using System;

namespace XWave.DTOs.Customers
{
    public class DiscountDto
    {
        public uint Percentage { get; set; }
        public bool IsActive { get; set; }
        public DateTime EndDate { get; set; }
    }
}