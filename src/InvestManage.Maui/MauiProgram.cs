using Microsoft.Extensions.Logging;

using InvestManage.Maui.Configuration;
using InvestManage.Maui.Services;

namespace InvestManage.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton(new HttpClient
		{
			BaseAddress = ApiEndpoint.BaseAddress,
			Timeout = TimeSpan.FromSeconds(10)
		});
		builder.Services.AddSingleton<IInvestManageApiClient, InvestManageApiClient>();
		builder.Services.AddSingleton<MainPage>();
		builder.Services.AddSingleton<AppShell>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
