using System.Net.Http;
using InvestManage.Client.Services;

namespace InvestManage.Wpf.ViewModels;

public abstract class OperationalViewModel : ObservableObject
{
    private bool isBusy;
    private string statusMessage = "Ready";
    private string? errorMessage;

    public bool IsBusy
    {
        get => isBusy;
        private set => SetProperty(ref isBusy, value);
    }

    public string StatusMessage
    {
        get => statusMessage;
        protected set => SetProperty(ref statusMessage, value);
    }

    public string? ErrorMessage
    {
        get => errorMessage;
        protected set => SetProperty(ref errorMessage, value);
    }

    protected async Task RunAsync(Func<Task> operation, string busyMessage)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        StatusMessage = busyMessage;
        try
        {
            await operation();
        }
        catch (Exception exception)
        {
            ErrorMessage = exception switch
            {
                ApiProblemException problem => problem.ToDisplayMessage(),
                HttpRequestException => "The API could not be reached. Confirm that it is running and try again.",
                TaskCanceledException => "The API request timed out. Try again.",
                _ => exception.Message
            };
            StatusMessage = "The operation failed.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
