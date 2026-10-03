namespace InvestManage.Contracts;

public sealed record SystemStatusResponse(
    string Service,
    string Version,
    string Environment,
    DateTimeOffset ServerTimeUtc);

