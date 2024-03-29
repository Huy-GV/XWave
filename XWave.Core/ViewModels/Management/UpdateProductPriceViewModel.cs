﻿using System.ComponentModel.DataAnnotations;

namespace XWave.Core.ViewModels.Management;

public class UpdateProductPriceViewModel
{
    [Required] 
    public uint UpdatedPrice { get; set; }

    public DateTime? Schedule { get; set; } = null;
}