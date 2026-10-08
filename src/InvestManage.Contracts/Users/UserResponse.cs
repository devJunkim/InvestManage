namespace InvestManage.Contracts.Users;

public sealed record UserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string DisplayName,
    string Email,
    string LoginId);
