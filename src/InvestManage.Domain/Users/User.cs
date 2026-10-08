using InvestManage.Domain.Common;

namespace InvestManage.Domain.Users;

public sealed class User
{
    public User(
        Guid id,
        string firstName,
        string lastName,
        string email,
        string normalizedEmail,
        string loginId,
        string normalizedLoginId,
        string passwordHash)
    {
        Id = Guard.Required(id, nameof(id));
        FirstName = Guard.Required(firstName, nameof(firstName));
        LastName = Guard.Required(lastName, nameof(lastName));
        Email = Guard.Required(email, nameof(email));
        NormalizedEmail = Guard.Required(normalizedEmail, nameof(normalizedEmail));
        LoginId = Guard.Required(loginId, nameof(loginId));
        NormalizedLoginId = Guard.Required(normalizedLoginId, nameof(normalizedLoginId));
        PasswordHash = Guard.Required(passwordHash, nameof(passwordHash));
        DisplayName = $"{FirstName} {LastName}";
    }

    public Guid Id { get; private set; }

    public string DisplayName { get; private set; }

    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public string Email { get; private set; }

    public string NormalizedEmail { get; private set; }

    public string LoginId { get; private set; }

    public string NormalizedLoginId { get; private set; }

    public string PasswordHash { get; private set; }
}
