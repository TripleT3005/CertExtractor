using Windows.Storage;

namespace CertExtractor.Helpers;

public static class SettingsHelper
{
    private static readonly ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;

    public static void SaveSetting<T>(string key, T value)
    {
        try
        {
            _localSettings.Values[key] = value;
        }
        catch
        {
            // Handle exceptions silently
        }
    }

    public static T ReadSetting<T>(string key, T defaultValue)
    {
        try
        {
            if (_localSettings.Values.ContainsKey(key))
            {
                return (T)_localSettings.Values[key]!;
            }
        }
        catch
        {
            // Handle exceptions silently
        }

        return defaultValue;
    }

    public static void DeleteSetting(string key)
    {
        try
        {
            _localSettings.Values.Remove(key);
        }
        catch
        {
            // Handle exceptions silently
        }
    }
}