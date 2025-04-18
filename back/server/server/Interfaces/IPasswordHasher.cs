namespace server.Interfaces;

public interface IPasswordHasher
{
    string Generate(string password);
    bool Verify(string hashed, string plain);
}