using server.Models;

namespace server.Interfaces;

public interface IJwtProvider
{
    string GenerateUserToken(Client client);
}
