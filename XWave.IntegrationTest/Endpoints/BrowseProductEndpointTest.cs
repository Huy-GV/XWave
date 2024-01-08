using System.Net.Http.Json;
using System.Net;
using Xunit;
using XWave.Core.DTOs.Management;
using FluentAssertions;
using XWave.IntegrationTest.Factories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Text;
using XWave.IntegrationTest.Utils;
using System.Text.Json;
using XWave.Core.DTOs.Shared;
using IdentityModel.Client;

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
    public async Task GetAllProductsForStaff_ShouldReturnUnauthorized_IfUserIsUnauthenticated()
    {
        var httpClient = XWaveApplicationFactory.CreateClient();
        var response = await httpClient.GetAsync("/api/product/private");

        var responseBody = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        responseBody.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task GetAllProductsForStaff_ShouldReturnUnauthorized_IfUserIsCustomer()
    {
        using var scope = CreateScope();
        using var dbContext = CreateDbContext(scope);
        var httpClient = XWaveApplicationFactory.CreateClient();

        var password = XWaveApplicationFactory.Services
            .GetRequiredService<IConfiguration>()
            .GetValue<string>("SeedData:Password");

        var query =
            from user in dbContext.Users
            join userRole in dbContext.UserRoles on user.Id equals userRole.UserId
            join role in dbContext.Roles on userRole.RoleId equals role.Id
            where role.Name == "Customer"
            select user.UserName;

        var userNames = await query.ToListAsync();

        var requests = userNames
            .Select(username => new
            {
                username,
                password
            })
            .Select(x => new StringContent(
                JsonSerializer.Serialize(x),
                Encoding.UTF8,
                "application/json"));

        foreach (var request in requests)
        {
            var authResponse = await httpClient.PostAsync("/api/auth/login", request);
            authResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var deserializedBody = await authResponse.Content.ReadFromJsonAsync<JwtTokenDto>(JsonUtil.CaseInsensitiveOptions);
            var token = deserializedBody?.Token;
            token.Should().NotBeNullOrEmpty();

            httpClient.SetBearerToken(token);
            var browseResponse = await httpClient.GetAsync("/api/product/private");
            browseResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            var browseResponseBody = await browseResponse.Content.ReadAsStringAsync();
            browseResponseBody.Should().BeNullOrEmpty();
        }
    }

    [Fact]
    public async Task GetAllProductsForStaff_ShouldReturnSuccess_IfUserIsStaff()
    {
        using var scope = CreateScope();
        using var dbContext = CreateDbContext(scope);
        var httpClient = XWaveApplicationFactory.CreateClient();

        var password = XWaveApplicationFactory.Services
            .GetRequiredService<IConfiguration>()
            .GetValue<string>("SeedData:Password");

        var query =
            from user in dbContext.Users
            join userRole in dbContext.UserRoles on user.Id equals userRole.UserId
            join role in dbContext.Roles on userRole.RoleId equals role.Id
            where role.Name == "Staff" || role.Name == "Manager"
            select user.UserName;

        var userNames = await query.ToListAsync();

        var requests = userNames
            .Select(username => new
            {
                username,
                password
            })
            .Select(x => new StringContent(
                JsonSerializer.Serialize(x),
                Encoding.UTF8,
                "application/json"));

        foreach (var request in requests)
        {
            var authResponse = await httpClient.PostAsync("/api/auth/login", request);
            authResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var deserializedBody = await authResponse.Content.ReadFromJsonAsync<JwtTokenDto>(JsonUtil.CaseInsensitiveOptions);
            var token = deserializedBody?.Token;
            token.Should().NotBeNullOrEmpty();

            httpClient.SetBearerToken(token);
            var browseResponse = await httpClient.GetAsync("/api/product/private");
            browseResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var browseResponseBody = await browseResponse.Content.ReadAsStringAsync();
            browseResponseBody.Should().NotBeEmpty();
        }
    }
}