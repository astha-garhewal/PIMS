using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PIMS.Application.DTOs.Auth;
using PIMS.Application.DTOs.Inventory;
using PIMS.Application.DTOs.Products;
using PIMS.Domain.Entities;
using PIMS.Infrastructure.Data;

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

    [Fact]
    public async Task User_Can_Get_Products()
    {
        var token = await RegisterAndLoginAsync(
            $"user_{Guid.NewGuid():N}");

        using var client = CreateAuthenticatedClient(token);

        var response = await client.GetAsync("/api/v1/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task User_Cannot_Adjust_Product_Price()
    {
        var token = await RegisterAndLoginAsync(
            $"user_{Guid.NewGuid():N}");

        using var client = CreateAuthenticatedClient(token);

        var response = await client.PutAsJsonAsync(
            "/api/v1/products/999999/price",
            new PriceAdjustmentDto
            {
                Value = 10,
                AdjustmentType = "fixed"
            });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Administrator_Can_Access_Price_Adjustment_Endpoint()
    {
        var username = $"admin_{Guid.NewGuid():N}";

        await RegisterAndLoginAsync(username);
        await PromoteUserToAdministratorAsync(username);

        var token = await LoginAsync(username);
        using var client = CreateAuthenticatedClient(token);

        var response = await client.PutAsJsonAsync(
            "/api/v1/products/999999/price",
            new PriceAdjustmentDto
            {
                Value = 10,
                AdjustmentType = "fixed"
            });

        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task User_Cannot_Perform_Inventory_Audit()
    {
        var token = await RegisterAndLoginAsync(
            $"user_{Guid.NewGuid():N}");

        using var client = CreateAuthenticatedClient(token);

        var response = await client.PostAsJsonAsync(
            "/api/v1/inventory/999999/audits",
            new InventoryAuditDto
            {
                AdjustedQuantity = 10,
                Reason = "Test audit"
            });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Administrator_Can_Access_Inventory_Audit_Endpoint()
    {
        var username = $"admin_{Guid.NewGuid():N}";

        await RegisterAndLoginAsync(username);
        await PromoteUserToAdministratorAsync(username);

        var token = await LoginAsync(username);
        using var client = CreateAuthenticatedClient(token);

        var response = await client.PostAsJsonAsync(
            "/api/v1/inventory/999999/audits",
            new InventoryAuditDto
            {
                AdjustedQuantity = 10,
                Reason = "Test audit"
            });

        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<string> RegisterAndLoginAsync(
        string username,
        string password = "Test@12345")
    {
        await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterDto
            {
                Username = username,
                Email = $"{username}@example.com",
                Password = password
            });

        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginDto
            {
                Username = username,
                Password = password
            });

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content
                .ReadFromJsonAsync<LoginResponseDto>();

        return result!.Token;
    }

    private async Task PromoteUserToAdministratorAsync(string username)
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var user = await dbContext.Users
            .FirstAsync(user => user.Username == username);

        var adminRole = await dbContext.Roles
            .FirstAsync(role => role.RoleName == "Administrator");

        var existingRoles = await dbContext.UserRoles
            .Where(userRole => userRole.UserID == user.UserID)
            .ToListAsync();

        dbContext.UserRoles.RemoveRange(existingRoles);
        dbContext.UserRoles.Add(new UserRole
        {
            UserID = user.UserID,
            RoleID = adminRole.RoleID
        });

        await dbContext.SaveChangesAsync();
    }

    private async Task<string> LoginAsync(
        string username,
        string password = "Test@12345")
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginDto
            {
                Username = username,
                Password = password
            });

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content
                .ReadFromJsonAsync<LoginResponseDto>();

        return result!.Token;
    }

    private HttpClient CreateAuthenticatedClient(string token)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
