using PIMS.Application.DTOs.Auth;

namespace PIMS.Application.Interfaces;

public interface IAuthService
{
    Task<UserResponseDto> RegisterAsync(RegisterDto dto);
}
