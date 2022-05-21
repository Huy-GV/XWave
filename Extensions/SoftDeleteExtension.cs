using System;
using XWave.Models;

namespace XWave.Extensions;

public static class SoftDeleteExtension
{
    public static void SoftDelete<TEntity>(this TEntity entity) where TEntity : ISoftDeletable
    {
        entity.DeleteDate = DateTime.Now;
        entity.IsDeleted = true;
    }
}