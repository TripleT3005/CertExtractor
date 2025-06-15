using CertExtractor.ViewModels;
using CertExtractor.Views;
using Microsoft.UI.Xaml;
using System;
using System.IO;

namespace CertExtractor;

public sealed partial class MainWindow : Window
{
    public MainWindowViewModel ViewModel { get; }
    public MainWindow()
    {
        this.InitializeComponent();

        App.SetMainWindow(this);

        ViewModel = new MainWindowViewModel();

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

        appWindow.Title = "CertExtractor";
        appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 800, Height = 800 });

        // Set custom title bar
        string iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcons", "CertExtractor.ico");
        if (File.Exists(iconPath))
            appWindow.SetIcon(iconPath);
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(TitleBarGrid);

        //if (appWindow.Presenter is OverlappedPresenter presenter)
        //{
        //    presenter.IsMaximizable = true;
        //    presenter.IsMinimizable = true;
        //    presenter.IsResizable = true;
        //}

        this.Activated += MainWindow_Activated;

        // Navigate to main page
        ContentFrame.Navigate(typeof(MainPage));
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs e)
    {
        ViewModel.SetToggleIcon();
        this.Activated -= MainWindow_Activated;
    }

}
