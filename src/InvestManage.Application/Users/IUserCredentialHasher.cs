namespace InvestManage.Application.Users;

public interface IUserCredentialHasher
{
    string Hash(string password);

    bool Verify(string passwordHash, string password);
}
