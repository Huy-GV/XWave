namespace XWave.Core.Models;

public interface ISoftDeletable
{
    public bool IsDeleted { get; set; }
    public DateTime? DeleteDate { get; set; }
}