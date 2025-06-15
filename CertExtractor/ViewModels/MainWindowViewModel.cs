using CertExtractor.Commands;
using Microsoft.UI.Xaml;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CertExtractor.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    // Backing Fields
    private string _themeToggleIcon = string.Empty;

    // Properties
    public string ThemeToggleIcon
    {
        get => _themeToggleIcon;
        set => SetProperty(ref _themeToggleIcon, value);
    }

    // Commands
    public ICommand ToggleThemeCommand { get; }

    public MainWindowViewModel()
    {
        // Initialize commands
        ToggleThemeCommand = new RelayCommand(async () => await ExecuteToggleThemeAsync());




    }

    public void SetToggleIcon()
    {
        if (App.ThemeService != null)
        {
            ThemeToggleIcon = App.ThemeService.GetCurrentTheme() == ElementTheme.Light ? "🌙" : "☀️";
        }
        else
        {
            ThemeToggleIcon = "❓";
        }
    }
    private async Task ExecuteToggleThemeAsync()
    {
        if (App.ThemeService != null)
        {
            App.ThemeService.ToggleTheme();
            SetToggleIcon();
        }
    }




}
