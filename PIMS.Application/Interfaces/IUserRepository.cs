using PIMS.Domain.Entities;

namespace PIMS.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);

    Task<User?> GetByEmailAsync(string email);

    Task<Role?> GetRoleByNameAsync(string roleName);

    Task AddAsync(User user);

    Task AddUserRoleAsync(UserRole userRole);
}
