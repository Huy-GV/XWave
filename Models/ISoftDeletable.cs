using System;

namespace XWave.Models
{
    public interface ISoftDeletable
    {
        public bool IsDeleted { get; set; }
        public DateTime? DeleteDate { get; set; }

        public void SoftDelete()
        {
            DeleteDate = DateTime.Now;
            IsDeleted = true;
        }
    }
}