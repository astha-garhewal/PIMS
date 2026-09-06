using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using PIMS.Application.DTOs.Auth;

namespace PIMS.Tests;

public class AuthIntegrationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthIntegrationTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ReturnsCreated()
    {
        var request = new RegisterDto
        {
            Username = $"testuser_{Guid.NewGuid():N}",
            Email = $"test_{Guid.NewGuid():N}@example.com",
            Password = "Test@12345"
        };

        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            request);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var username = $"loginuser_{Guid.NewGuid():N}";
        var email = $"login_{Guid.NewGuid():N}@example.com";
        var password = "Test@12345";

        await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterDto
            {
                Username = username,
                Email = email,
                Password = password
            });

        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginDto
            {
                Username = username,
                Password = password
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result =
            await response.Content.ReadFromJsonAsync<LoginResponseDto>();

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result!.Token));
        Assert.Equal(username, result.Username);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsBadRequest()
    {
        var username = $"invalid_{Guid.NewGuid():N}";

        await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterDto
            {
                Username = username,
                Email = $"{username}@example.com",
                Password = "Correct@123"
            });

        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginDto
            {
                Username = username,
                Password = "Wrong@123"
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetProducts_WithoutToken_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/products");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetProducts_WithValidToken_ReturnsSuccess()
    {
        var username = $"productuser_{Guid.NewGuid():N}";
        var password = "Test@12345";

        await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterDto
            {
                Username = username,
                Email = $"{username}@example.com",
                Password = password
            });

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginDto
            {
                Username = username,
                Password = password
            });

        var loginResult =
            await loginResponse.Content
                .ReadFromJsonAsync<LoginResponseDto>();

        Assert.NotNull(loginResult);

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                loginResult!.Token);

        var response = await client.GetAsync("/api/v1/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
