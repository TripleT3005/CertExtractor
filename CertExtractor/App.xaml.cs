using CertExtractor.Services;
using Microsoft.UI.Xaml;

namespace CertExtractor; 

public partial class App : Application
{
    private Window? _mainWindow;
    public static Window? MainWindow { get; private set; }
    public static ThemeService ThemeService { get; private set; } = null!;


    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        _mainWindow = new MainWindow();
        App.MainWindow = _mainWindow;

        // Initialize services
        ThemeService = new ThemeService();
        ThemeService.Initialize(_mainWindow);

        _mainWindow.Activate();
    }

    public static void SetMainWindow(Window window)
    {
        MainWindow = window;
    }
}
