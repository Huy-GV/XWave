using Microsoft.AspNetCore.Mvc;
using XWave.Data;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using XWave.Models;

namespace XWave.Controllers
{
    public class AbstractController<T> : ControllerBase where T : ControllerBase
    {
        protected XWaveDbContext DbContext { get; }
        protected ILogger<T> Logger { get; }
        public AbstractController(
            XWaveDbContext dbContext,
            ILogger<T> logger)
        {
            DbContext = dbContext;
            Logger = logger;
        }

        protected async Task<bool> ItemExistsAsync<T>(int id)
        {
            var entityTypeName = typeof(T).Name;
            switch (entityTypeName)
            {
                case nameof(Product):
                    return await DbContext.Product.FindAsync(id) != null;
                case nameof(Category):
                    return await DbContext.Category.FindAsync(id) != null;
                case nameof(Discount):
                    return await DbContext.Discount.FindAsync(id) != null;
                default:
                    Logger.LogError($"Entity with type {entityTypeName} not found");
                    return false;
            }
        }
    }
}
