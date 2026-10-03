using InvestManage.Api.Controllers;
using InvestManage.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace InvestManage.Api.Tests;

public sealed class SystemControllerTests
{
    [Fact]
    public void GetStatus_ReturnsServiceInformation()
    {
        var controller = new SystemController(new TestHostEnvironment());

        var result = controller.GetStatus();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var status = Assert.IsType<SystemStatusResponse>(ok.Value);
        Assert.Equal("InvestManage API", status.Service);
        Assert.Equal(Environments.Development, status.Environment);
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "InvestManage.Api.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}

