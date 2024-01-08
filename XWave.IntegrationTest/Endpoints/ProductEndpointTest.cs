using System.Net.Http.Json;
using System.Net;
using Xunit;
using XWave.Core.DTOs.Management;
using FluentAssertions;
using XWave.IntegrationTest.Factories;

namespace XWave.IntegrationTest.Endpoints;
public class BrowseProductEndpointTests : BaseTest
{
    public BrowseProductEndpointTests(XWaveApiWebApplicationFactory webApplicationFactory) : base(webApplicationFactory)
    { 
    }

    [Fact]
    public async Task GetAllProducts_ShouldReturnProductList()
    {
        var httpClient = XWaveApplicationFactory.CreateClient();
        var response = await httpClient.GetAsync("/api/product/");

        var responseBody = await response.Content.ReadFromJsonAsync<IEnumerable<DetailedProductDto>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        responseBody.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAllProductsForStaff_ShouldReturnUnauthorized()
    {
        var httpClient = XWaveApplicationFactory.CreateClient();
        var response = await httpClient.GetAsync("/api/product/private");

        var responseBody = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        responseBody.Should().BeNullOrEmpty();
    }
}