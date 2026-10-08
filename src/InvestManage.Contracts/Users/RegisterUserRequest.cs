namespace InvestManage.Contracts.Users;

public sealed record RegisterUserRequest(
    string? FirstName,
    string? LastName,
    string? Email,
    string? LoginId,
    string? Password);
