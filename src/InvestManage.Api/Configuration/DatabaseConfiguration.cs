namespace InvestManage.Api.Configuration;

public static class DatabaseConfiguration
{
    public const string ConnectionStringName = "InvestManage";

    public static string? GetConnectionString(
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            if (environment.IsDevelopment())
            {
                return null;
            }

            throw new InvalidOperationException(
                $"ConnectionStrings:{ConnectionStringName} must be configured outside Development.");
        }

        if (connectionString.Contains("REPLACE_WITH_", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"ConnectionStrings:{ConnectionStringName} contains placeholder credentials.");
        }

        return connectionString;
    }
}
