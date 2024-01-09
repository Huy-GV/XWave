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
using XWave.Core.DTOs.Customers;

namespace XWave.IntegrationTest.Endpoints;
public class BrowseProductEndpointTests : BaseTest
{
    public BrowseProductEndpointTests(XWaveApiWebApplicationFactory webApplicationFactory) : base(webApplicationFactory)
    { 
    }

    [Fact]
    public async Task GetAllProducts_ShouldReturnProductList()
    {
        await using var scope = CreateScope();
        await using var dbContext = CreateDbContext(scope);

        var storedProductIds = await dbContext.Product
            .Where(x => !x.IsDeleted && !x.IsDiscontinued)
            .Select(x => x.Id)
            .ToListAsync();

        var httpClient = XWaveApplicationFactory.CreateClient();
        var response = await httpClient.GetAsync("/api/product/");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await response.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>() 
            ?? Enumerable.Empty<ProductDto>();
        products.Select(x => x.Id).Should().BeEquivalentTo(storedProductIds);
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
        await using var scope = CreateScope();
        await using var dbContext = CreateDbContext(scope);
        var httpClient = XWaveApplicationFactory.CreateClient();

        var password = XWaveApplicationFactory.Services
            .GetRequiredService<IConfiguration>()
            .GetValue<string>("SeedData:Password");

        var getCustomerUserNamesQuery =
            from user in dbContext.Users
            join userRole in dbContext.UserRoles on user.Id equals userRole.UserId
            join role in dbContext.Roles on userRole.RoleId equals role.Id
            where role.Name == "Customer"
            select user.UserName;

        var userNames = await getCustomerUserNamesQuery.ToListAsync();

        var requests = userNames
            .Select(username => new StringContent(
                JsonSerializer.Serialize(new
                {
                    username,
                    password
                }),
                Encoding.UTF8,
                "application/json"));

        foreach (var request in requests)
        {
            var token = await GivenTokenFromSuccessfulAuthentication(httpClient, request);

            httpClient.SetBearerToken(token);
            var browseResponse = await httpClient.GetAsync("/api/product/private");
            browseResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            var browseResponseBody = await browseResponse.Content.ReadAsStringAsync();
            browseResponseBody.Should().BeNullOrEmpty();
        }
    }

    [Fact]
    public async Task GetAllProductsForStaff_ShouldSucceed_IfUserIsStaff()
    {
        await using var scope = CreateScope();
        await using var dbContext = CreateDbContext(scope);

        var storedProductIds = await dbContext.Product
            .Where(x => !x.IsDeleted && !x.IsDiscontinued)
            .Select(x => x.Id)
            .ToListAsync();

        var httpClient = XWaveApplicationFactory.CreateClient();
        var password = XWaveApplicationFactory.Services
            .GetRequiredService<IConfiguration>()
            .GetValue<string>("SeedData:Password");

        var getStaffUserNamesQuery =
            from user in dbContext.Users
            join userRole in dbContext.UserRoles on user.Id equals userRole.UserId
            join role in dbContext.Roles on userRole.RoleId equals role.Id
            where role.Name == "Staff" || role.Name == "Manager"
            select user.UserName;

        var userNames = await getStaffUserNamesQuery.ToListAsync();

        var requests = userNames
            .Select(username => new StringContent(
                JsonSerializer.Serialize(new
                {
                    username,
                    password
                }),
                Encoding.UTF8,
                "application/json"));

        foreach (var request in requests)
        {
            var token = await GivenTokenFromSuccessfulAuthentication(httpClient, request);

            httpClient.SetBearerToken(token);

            var browseResponse = await httpClient.GetAsync("/api/product/private");

            browseResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var browseResponseBody = await browseResponse.Content.ReadAsStringAsync();
            var products = await browseResponse.Content.ReadFromJsonAsync<IEnumerable<DetailedProductDto>>()
                ?? Enumerable.Empty<DetailedProductDto>();

            products.Select(x => x.Id).Should().BeEquivalentTo(storedProductIds);
            products.Select(x => x.LatestRestock).Should().NotBeNull();
        }
    }

    private static async Task<string> GivenTokenFromSuccessfulAuthentication(
        HttpClient httpClient,
        HttpContent httpContent)
    {
        var authResponse = await httpClient.PostAsync("/api/auth/login", httpContent);
        authResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var deserializedBody = await authResponse.Content.ReadFromJsonAsync<JwtTokenDto>(JsonUtil.CaseInsensitiveOptions);
        var token = deserializedBody?.Token;
        token.Should().NotBeNullOrEmpty();

        return token!;
    }
}