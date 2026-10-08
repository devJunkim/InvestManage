using InvestManage.Application.Common;
using InvestManage.Application.Users;
using InvestManage.Domain.Users;

namespace InvestManage.Application.Tests.Users;

public sealed class UserServiceTests
{
    [Fact]
    public async Task Register_NormalizesIdentityAndHashesPassword()
    {
        var repository = new FakeUserRepository();
        var service = new UserService(repository, new FakeHasher());

        var user = await service.RegisterAsync(
            "  Jun ", " Kim  ", " Jun@example.com ", " JunK ", "password123");

        Assert.Equal("Jun Kim", user.DisplayName);
        Assert.Equal("JUN@EXAMPLE.COM", user.NormalizedEmail);
        Assert.Equal("JUNK", user.NormalizedLoginId);
        Assert.Equal("hashed:password123", user.PasswordHash);
        Assert.Same(user, repository.AddedUser);
        Assert.True(repository.Saved);
    }

    [Fact]
    public async Task Register_RejectsDuplicateEmailAndLoginId()
    {
        var repository = new FakeUserRepository { EmailExists = true, LoginIdExists = true };
        var service = new UserService(repository, new FakeHasher());

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() =>
            service.RegisterAsync("Jun", "Kim", "jun@example.com", "junk", "password123"));

        Assert.Contains("email", exception.Errors.Keys);
        Assert.Contains("loginId", exception.Errors.Keys);
    }

    [Fact]
    public async Task Login_AcceptsEmailOrLoginIdWithCorrectPassword()
    {
        var repository = new FakeUserRepository();
        var service = new UserService(repository, new FakeHasher());
        var registered = await service.RegisterAsync("Jun", "Kim", "jun@example.com", "junk", "password123");
        repository.UserToFind = registered;

        var byEmail = await service.LoginAsync("JUN@example.com", "password123");
        var byLoginId = await service.LoginAsync("junk", "password123");

        Assert.Same(registered, byEmail);
        Assert.Same(registered, byLoginId);
    }

    [Theory]
    [InlineData("unknown", "password123")]
    [InlineData("junk", "wrong-password")]
    public async Task Login_RejectsInvalidCredentials(string identifier, string password)
    {
        var repository = new FakeUserRepository();
        var service = new UserService(repository, new FakeHasher());
        if (identifier == "junk")
        {
            repository.UserToFind = await service.RegisterAsync(
                "Jun", "Kim", "jun@example.com", "junk", "password123");
        }

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() =>
            service.LoginAsync(identifier, password));

        Assert.Contains("credentials", exception.Errors.Keys);
    }

    private sealed class FakeHasher : IUserCredentialHasher
    {
        public string Hash(string password) => $"hashed:{password}";

        public bool Verify(string passwordHash, string password) => passwordHash == Hash(password);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public bool EmailExists { get; init; }
        public bool LoginIdExists { get; init; }
        public User? UserToFind { get; set; }
        public User? AddedUser { get; private set; }
        public bool Saved { get; private set; }

        public Task<User?> FindByIdentifierAsync(string normalizedIdentifier, CancellationToken cancellationToken) =>
            Task.FromResult(UserToFind);

        public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken) =>
            Task.FromResult(EmailExists);

        public Task<bool> LoginIdExistsAsync(string normalizedLoginId, CancellationToken cancellationToken) =>
            Task.FromResult(LoginIdExists);

        public void Add(User user) => AddedUser = user;

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            Saved = true;
            return Task.CompletedTask;
        }
    }
}
