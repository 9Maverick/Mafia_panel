using Mafia_panel.Core;
using Mafia_panel.Models;
using Mafia_panel.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace Mafia_panel;

public partial class App : Application
{
	public static IHost? Host { get; private set; }

	public App()
	{
		Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
			.ConfigureServices((hostContext, services) =>
			{
				services.AddSingleton<IPlayersViewModel, PlayersViewModel>(); 
				services.AddSingleton<IGameModeModel, GameModeModel>();
				services.AddSingleton<IDiscordClientModel, DiscordClientModel>();
				services.AddSingleton<IMainViewModel, MainViewModel>();
				services.AddSingleton<MainWindow>();
				services.AddSingleton<InitialViewModel>();
				services.AddSingleton<DayViewModel>();
				services.AddSingleton<NightViewModel>();
			})
			.Build();
	}

	protected override async void OnStartup(StartupEventArgs e)
	{
		await Host!.StartAsync();

		var mainWindow = Host.Services.GetRequiredService<MainWindow>();
		mainWindow.Show();
		base.OnStartup(e);
	}

	protected override async void OnExit(ExitEventArgs e)
	{
		await Host!.StopAsync();
		base.OnExit(e);
	}
}
