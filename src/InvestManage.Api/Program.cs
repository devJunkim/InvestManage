using InvestManage.Api.Errors;
using InvestManage.Api.Security;
using InvestManage.Application.Accounts;
using InvestManage.Application.Investments;
using InvestManage.Application.Users;
using InvestManage.Infrastructure;
using InvestManage.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    // Re-add higher-priority providers after the ignored local file so secrets,
    // environment variables, and command-line values always win.
    builder.Configuration
        .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
        .AddUserSecrets<Program>(optional: true)
        .AddEnvironmentVariables()
        .AddCommandLine(args);
}

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddScoped<InvestmentAccountService>();
builder.Services.AddScoped<InvestmentItemService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddSingleton<IUserCredentialHasher, UserCredentialHasher>();

var connectionString = DatabaseConfiguration.GetConnectionString(
    builder.Configuration,
    builder.Environment);
if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddInvestManageInfrastructure(connectionString);
}

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
