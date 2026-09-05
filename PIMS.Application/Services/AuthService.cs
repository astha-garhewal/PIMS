using Microsoft.AspNetCore.Identity;
using PIMS.Application.DTOs.Auth;
using PIMS.Application.Interfaces;
using PIMS.Domain.Entities;

namespace PIMS.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher<User> passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username))
        {
            throw new ArgumentException("Username is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            throw new ArgumentException("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new ArgumentException("Password is required.");
        }

        if (dto.Password.Length < 8)
        {
            throw new ArgumentException(
                "Password must contain at least 8 characters.");
        }

        var username = dto.Username.Trim();
        var email = dto.Email.Trim();

        var existingUsername =
            await _userRepository.GetByUsernameAsync(username);

        if (existingUsername != null)
        {
            throw new ArgumentException("Username already exists.");
        }

        var existingEmail =
            await _userRepository.GetByEmailAsync(email);

        if (existingEmail != null)
        {
            throw new ArgumentException("Email already exists.");
        }

        var user = new User
        {
            Username = username,
            Email = email,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(user, dto.Password);

        await _userRepository.AddAsync(user);

        return new UserResponseDto
        {
            UserID = user.UserID,
            Username = user.Username,
            Email = user.Email
        };
    }
}
