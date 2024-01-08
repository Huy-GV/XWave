using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Dynamic;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using XWave.Core.Data;
using XWave.Core.Data.DatabaseSeeding.Factories;
using XWave.Core.DTOs.Shared;
using XWave.Core.Services.Communication;
using XWave.IntegrationTest.Factories;
using XWave.IntegrationTest.Utils;

namespace XWave.IntegrationTest.Endpoints;

public class AuthenticationEndpointTest : BaseTest
{
    public AuthenticationEndpointTest(XWaveApiWebApplicationFactory webApplicationFactory) : base(webApplicationFactory)
    {
    }

    [Fact]
    public async Task LogIn_ShouldSucceed_WhenPasswordIsCorrect()
    {
        using var dbContext = CreateDbContext();

        var password = XWaveApplicationFactory.Services
            .GetRequiredService<IConfiguration>()
            .GetValue<string>("SeedData:Password");

        var httpClient = XWaveApplicationFactory.CreateClient();

        var userNames = await dbContext.Users.Select(x => x.UserName).ToListAsync();
        var requests = userNames
            .Select(x => new
            {
                username = x,
                password
            })
            .Select(x => new StringContent(
                JsonSerializer.Serialize(x),
                Encoding.UTF8,
                "application/json"));

        foreach (var requestContent in requests)
        {
            var response = await httpClient.PostAsync("/api/auth/login", requestContent);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var deserializedBody = await response.Content.ReadFromJsonAsync<JwtTokenDto>(JsonUtil.CaseInsensitiveOptions);

            var token = deserializedBody?.Token ?? string.Empty;
            token.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task LogIn_ShouldFaild_WhenPasswordIsIncorrect()
    {
        using var dbContext = CreateDbContext();

        var password = XWaveApplicationFactory.Services
            .GetRequiredService<IConfiguration>()
            .GetValue<string>("SeedData:Password");

        var httpClient = XWaveApplicationFactory.CreateClient();

        var faker = new Faker();
        var userNames = await dbContext.Users.Select(x => x.UserName).ToListAsync();
        var requests = userNames
            .Select(x => new
            {
                username = faker.Random.Bool() ? x : faker.Random.Word(),
                password = faker.Random.WordsArray(20).Where(x => x != password).First()
            })
            .Select(x => new StringContent(
                JsonSerializer.Serialize(x),
                Encoding.UTF8,
                "application/json"));

        foreach (var request in requests)
        {
            var response = await httpClient.PostAsync("/api/auth/login", request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            var responseBody = await response.Content.ReadFromJsonAsync<Error>(JsonUtil.CaseInsensitiveOptions);
            responseBody!.Code.Should().BeOneOf(ErrorCode.AuthenticationError, ErrorCode.AuthorizationError);
        }
    }
}
