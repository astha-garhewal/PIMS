using PIMS.Domain.Entities;

namespace PIMS.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}
