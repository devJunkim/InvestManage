namespace InvestManage.Maui.Configuration;

public static class ApiEndpoint
{
    public static Uri BaseAddress
    {
        get
        {
#if ANDROID
            return new Uri("http://10.0.2.2:5254");
#else
            return new Uri("https://localhost:7094");
#endif
        }
    }
}

