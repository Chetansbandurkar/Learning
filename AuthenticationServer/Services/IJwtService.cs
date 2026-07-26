using AuthenticationServer.Models;

namespace AuthenticationServer.Services;

public interface IJwtService
{
    string GenerateToken(User user);
}