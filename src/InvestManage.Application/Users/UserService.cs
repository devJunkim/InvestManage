using System.Net.Mail;
using InvestManage.Application.Common;
using InvestManage.Domain.Users;

namespace InvestManage.Application.Users;

public sealed class UserService(IUserRepository repository, IUserCredentialHasher credentialHasher)
{
    public async Task<User> RegisterAsync(
        string? firstName,
        string? lastName,
        string? email,
        string? loginId,
        string? password,
        CancellationToken cancellationToken = default)
    {
        var details = ValidateRegistration(firstName, lastName, email, loginId, password);
        var errors = new Dictionary<string, string[]>();

        if (await repository.EmailExistsAsync(details.NormalizedEmail, cancellationToken))
        {
            errors["email"] = ["That email address is already registered."];
        }

        if (await repository.LoginIdExistsAsync(details.NormalizedLoginId, cancellationToken))
        {
            errors["loginId"] = ["That Login ID is already registered."];
        }

        if (errors.Count > 0)
        {
            throw new ApplicationValidationException(errors);
        }

        var user = new User(
            Guid.NewGuid(),
            details.FirstName,
            details.LastName,
            details.Email,
            details.NormalizedEmail,
            details.LoginId,
            details.NormalizedLoginId,
            credentialHasher.Hash(details.Password));

        repository.Add(user);
        await repository.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<User> LoginAsync(
        string? identifier,
        string? password,
        CancellationToken cancellationToken = default)
    {
        var normalizedIdentifier = Normalize(identifier);
        if (normalizedIdentifier.Length == 0 || string.IsNullOrEmpty(password))
        {
            throw InvalidCredentials();
        }

        var user = await repository.FindByIdentifierAsync(normalizedIdentifier, cancellationToken);
        if (user is null || !credentialHasher.Verify(user.PasswordHash, password))
        {
            throw InvalidCredentials();
        }

        return user;
    }

    private static RegistrationDetails ValidateRegistration(
        string? firstName,
        string? lastName,
        string? email,
        string? loginId,
        string? password)
    {
        var normalizedFirstName = firstName?.Trim() ?? string.Empty;
        var normalizedLastName = lastName?.Trim() ?? string.Empty;
        var normalizedEmail = email?.Trim() ?? string.Empty;
        var normalizedLoginId = loginId?.Trim() ?? string.Empty;
        var errors = new Dictionary<string, string[]>();

        ValidateRequiredLength(errors, "firstName", "First name", normalizedFirstName, 100);
        ValidateRequiredLength(errors, "lastName", "Last name", normalizedLastName, 100);
        ValidateRequiredLength(errors, "loginId", "Login ID", normalizedLoginId, 100);

        if (normalizedEmail.Length == 0 || normalizedEmail.Length > 320 || !IsValidEmail(normalizedEmail))
        {
            errors["email"] = ["Enter a valid email address."];
        }

        if (password is null || password.Length < 8)
        {
            errors["password"] = ["The password must contain at least 8 characters."];
        }
        else if (password.Length > 128)
        {
            errors["password"] = ["The password cannot exceed 128 characters."];
        }

        if (errors.Count > 0)
        {
            throw new ApplicationValidationException(errors);
        }

        return new RegistrationDetails(
            normalizedFirstName,
            normalizedLastName,
            normalizedEmail,
            Normalize(normalizedEmail),
            normalizedLoginId,
            Normalize(normalizedLoginId),
            password!);
    }

    private static void ValidateRequiredLength(
        IDictionary<string, string[]> errors,
        string key,
        string label,
        string value,
        int maximumLength)
    {
        if (value.Length == 0)
        {
            errors[key] = [$"{label} is required."];
        }
        else if (value.Length > maximumLength)
        {
            errors[key] = [$"{label} cannot exceed {maximumLength} characters."];
        }
    }

    private static bool IsValidEmail(string value)
    {
        try
        {
            return new MailAddress(value).Address.Equals(value, StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static string Normalize(string? value) => value?.Trim().ToUpperInvariant() ?? string.Empty;

    private static ApplicationValidationException InvalidCredentials() =>
        new(new Dictionary<string, string[]> { ["credentials"] = ["The email/Login ID or password is incorrect."] });

    private sealed record RegistrationDetails(
        string FirstName,
        string LastName,
        string Email,
        string NormalizedEmail,
        string LoginId,
        string NormalizedLoginId,
        string Password);
}
