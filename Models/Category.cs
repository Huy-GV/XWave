using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace XWave.Models
{
    public class Category : IEntity
    {
        public int ID { get; set; }
        [Required]
        [StringLength(10, MinimumLength =2)]
        public string Name { get; set; }
        [StringLength(50, MinimumLength =5)]
        public string Description { get; set; }
    }
}