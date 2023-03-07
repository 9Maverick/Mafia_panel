using Mafia_panel.ViewModels;
using System.Windows;

namespace Mafia_panel;

public partial class MainWindow : Window
{
	public MainWindow(MainViewModel mainViewModel)
	{
		DataContext = mainViewModel;
		mainViewModel.Window= this;
		mainViewModel.SwitchCurrentViewModelTo<InitialViewModel>();
		InitializeComponent();
	}
}
