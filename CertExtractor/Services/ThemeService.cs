using System;
using CertExtractor.Helpers;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Windows.UI.ViewManagement;

namespace CertExtractor.Services;

public class ThemeService
{
    private Window? _window;
    private ElementTheme _currentTheme = ElementTheme.Default;

    public ElementTheme GetCurrentTheme() => _currentTheme;

    public void Initialize(Window window)
    {
        _window = window;
        ApplySystemTheme();
        ApplySystemAccentColor();
    }

    public void ToggleTheme()
    {
        var newTheme = _currentTheme switch
        {
            ElementTheme.Light => ElementTheme.Dark,
            ElementTheme.Dark => ElementTheme.Light,
            _ => GetSystemTheme() == ElementTheme.Light ? ElementTheme.Dark : ElementTheme.Light
        };

        SetTheme(newTheme);
    }

    public void SetTheme(ElementTheme theme)
    {
        if (_window?.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = theme;
            _currentTheme = theme;

            SettingsHelper.SaveSetting("AppTheme", theme.ToString());
            UpdateTitleBarColors(theme);
        }
    }

    private void ApplySystemTheme()
    {
        var savedTheme = SettingsHelper.ReadSetting("AppTheme", "Default");
        if (Enum.TryParse<ElementTheme>(savedTheme, out var theme))
        {
            SetTheme(theme);
        }
        else
        {
            SetTheme(ElementTheme.Default);
        }
    }

    private void ApplySystemAccentColor()
    {
        try
        {
            var uiSettings = new UISettings();
            var accentColor = uiSettings.GetColorValue(UIColorType.Accent);

            if (_window?.Content is FrameworkElement rootElement)
            {
                rootElement.Resources["SystemAccentColor"] = accentColor;
                rootElement.Resources["SystemAccentColorLight1"] = LightenColor(accentColor, 0.15f);
                rootElement.Resources["SystemAccentColorLight2"] = LightenColor(accentColor, 0.3f);
                rootElement.Resources["SystemAccentColorDark1"] = DarkenColor(accentColor, 0.15f);
                rootElement.Resources["SystemAccentColorDark2"] = DarkenColor(accentColor, 0.3f);
            }
        }
        catch
        {
            // Fallback to default accent color if system color cannot be retrieved
        }
    }

    private ElementTheme GetSystemTheme()
    {
        var uiSettings = new UISettings();
        var backgroundColor = uiSettings.GetColorValue(UIColorType.Background);
        return backgroundColor == Colors.Black ? ElementTheme.Dark : ElementTheme.Light;
    }

    private void UpdateTitleBarColors(ElementTheme theme)
    {
        if (_window == null) return;

        var titleBar = _window.AppWindow.TitleBar;
        var isDark = theme == ElementTheme.Dark || (theme == ElementTheme.Default && GetSystemTheme() == ElementTheme.Dark);

        if (isDark)
        {
            titleBar.BackgroundColor = Colors.Black;
            titleBar.ForegroundColor = Colors.White;
            titleBar.InactiveBackgroundColor = Colors.Black;
            titleBar.InactiveForegroundColor = Colors.Gray;
        }
        else
        {
            titleBar.BackgroundColor = Colors.White;
            titleBar.ForegroundColor = Colors.Black;
            titleBar.InactiveBackgroundColor = Colors.White;
            titleBar.InactiveForegroundColor = Colors.Gray;
        }
    }

    private Windows.UI.Color LightenColor(Windows.UI.Color color, float amount)
    {
        return Windows.UI.Color.FromArgb(color.A,
            (byte)Math.Min(255, color.R + (255 - color.R) * amount),
            (byte)Math.Min(255, color.G + (255 - color.G) * amount),
            (byte)Math.Min(255, color.B + (255 - color.B) * amount));
    }

    private Windows.UI.Color DarkenColor(Windows.UI.Color color, float amount)
    {
        return Windows.UI.Color.FromArgb(color.A,
            (byte)(color.R * (1 - amount)),
            (byte)(color.G * (1 - amount)),
            (byte)(color.B * (1 - amount)));
    }
}