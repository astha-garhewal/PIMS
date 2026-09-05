using Microsoft.AspNetCore.Mvc;
using PIMS.Application.DTOs.Auth;
using PIMS.Application.Interfaces;

namespace PIMS.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var user = await _authService.RegisterAsync(dto);

        return CreatedAtAction(
            nameof(Register),
            new { id = user.UserID },
            user);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var response = await _authService.LoginAsync(dto);

        return Ok(response);
    }
}
