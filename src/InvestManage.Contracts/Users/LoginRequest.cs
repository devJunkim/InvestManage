namespace InvestManage.Contracts.Users;

public sealed record LoginRequest(string? Identifier, string? Password);
