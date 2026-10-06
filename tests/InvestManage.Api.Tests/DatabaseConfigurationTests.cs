using InvestManage.Api.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace InvestManage.Api.Tests;

public sealed class DatabaseConfigurationTests
{
    [Fact]
    public void GetConnectionString_AllowsMissingValueDuringDevelopment()
    {
        var configuration = new ConfigurationBuilder().Build();

        var result = DatabaseConfiguration.GetConnectionString(
            configuration,
            new TestHostEnvironment(Environments.Development));

        Assert.Null(result);
    }

    [Fact]
    public void GetConnectionString_RequiresValueOutsideDevelopment()
    {
        var configuration = new ConfigurationBuilder().Build();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            DatabaseConfiguration.GetConnectionString(
                configuration,
                new TestHostEnvironment(Environments.Production)));

        Assert.Contains("ConnectionStrings:InvestManage", exception.Message);
    }

    [Fact]
    public void GetConnectionString_RejectsPlaceholderCredentials()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:InvestManage"] =
                    "Server=localhost;Database=InvestManage;User ID=REPLACE_WITH_SQL_LOGIN;Password=REPLACE_WITH_PASSWORD"
            })
            .Build();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            DatabaseConfiguration.GetConnectionString(
                configuration,
                new TestHostEnvironment(Environments.Development)));

        Assert.Contains("placeholder credentials", exception.Message);
    }

    private sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "InvestManage.Api.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
