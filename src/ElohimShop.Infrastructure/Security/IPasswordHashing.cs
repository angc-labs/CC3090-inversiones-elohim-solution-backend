namespace ElohimShop.Infrastructure.Security;

public interface IPasswordHashing
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public class PasswordHashingService : IPasswordHashing
{
    public string HashPassword(string password)
    {
        return PasswordHashing.Hash(password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return PasswordHashing.Verify(password, hash);
    }
}
