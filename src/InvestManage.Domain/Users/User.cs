using InvestManage.Domain.Common;

namespace InvestManage.Domain.Users;

public sealed class User
{
    public User(Guid id, string displayName)
    {
        Id = Guard.Required(id, nameof(id));
        DisplayName = Guard.Required(displayName, nameof(displayName));
    }

    public Guid Id { get; private set; }

    public string DisplayName { get; private set; }
}
