namespace InvestManage.Client.Services;

public sealed class ApiProblemException(
    string message,
    IReadOnlyDictionary<string, string[]>? errors = null)
    : Exception(message)
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } =
        errors ?? new Dictionary<string, string[]>();

    public string ToDisplayMessage()
    {
        if (Errors.Count == 0)
        {
            return Message;
        }

        return string.Join(
            Environment.NewLine,
            Errors.SelectMany(error => error.Value.Select(item => $"{error.Key}: {item}")));
    }
}
