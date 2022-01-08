using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using XWave.Data;

namespace XWave.Controllers
{
    public class StaffActivityLogController : AbstractController<StaffActivityLogController>
    {
        public StaffActivityLogController(
            XWaveDbContext dbContext,
            ILogger<StaffActivityLogController> logger) : base(dbContext, logger)
        {

        }
    }
}
