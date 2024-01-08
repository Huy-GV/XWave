using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using XWave.Core.Data;
using XWave.IntegrationTest.Factories;

namespace XWave.IntegrationTest.Endpoints;
public abstract class BaseTest : IClassFixture<XWaveApiWebApplicationFactory>
{
    public BaseTest(XWaveApiWebApplicationFactory webApplicationFactory)
    {
        XWaveApplicationFactory = webApplicationFactory;
    }

    protected XWaveApiWebApplicationFactory XWaveApplicationFactory { get; }

    internal XWaveDbContext CreateDbContext()
    {
        return XWaveApplicationFactory.Services.GetRequiredService<XWaveDbContext>();
    }
}
