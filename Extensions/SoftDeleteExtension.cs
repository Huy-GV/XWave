using XWave.Models;

namespace XWave.Extensions
{
    public static class SoftDeleteExtension
    {
        public static void SoftDelete<TEntity>(this TEntity entity) where TEntity : ISoftDeletable
        {
            entity.SoftDelete();
        }
    }
}