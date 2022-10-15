﻿namespace XWave.Core.DTOs.Management;

public record DetailedDiscountDto
{
    public uint Percentage { get; init; }
    public bool IsActive => EndDate > DateTime.Today && DateTime.Today > StartDate;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}