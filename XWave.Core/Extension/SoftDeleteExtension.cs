﻿using XWave.Core.Models;

namespace XWave.Core.Extension;

public static class SoftDeleteExtension
{
    internal static void SoftDelete<TEntity>(this TEntity entity) where TEntity : ISoftDeletable
    {
        entity.DeleteDate = DateTime.Now;
        entity.IsDeleted = true;
    }
}