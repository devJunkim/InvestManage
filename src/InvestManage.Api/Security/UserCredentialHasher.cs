using InvestManage.Application.Users;
using Microsoft.AspNetCore.Identity;

namespace InvestManage.Api.Security;

public sealed class UserCredentialHasher : IUserCredentialHasher
{
    private static readonly object HashingUser = new();
    private readonly PasswordHasher<object> hasher = new();

    public string Hash(string password) => hasher.HashPassword(HashingUser, password);

    public bool Verify(string passwordHash, string password)
    {
        try
        {
            return hasher.VerifyHashedPassword(HashingUser, passwordHash, password) != PasswordVerificationResult.Failed;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
