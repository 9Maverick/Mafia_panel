using Mafia_panel.ViewModels;
using System.Windows;

namespace Mafia_panel;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        DataContext = new MainViewModel(this);
    }
}
