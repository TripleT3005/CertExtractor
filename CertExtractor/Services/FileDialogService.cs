using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace CertExtractor.Services;

public class FileDialogService
{
    private readonly IntPtr _mainWindowHandle;

    public FileDialogService(Microsoft.UI.Xaml.Window window)
    {
        _mainWindowHandle = WindowNative.GetWindowHandle(window);
    }

    public async Task<IReadOnlyList<StorageFile>> PickMultipleFilesAsync()
    {
        var picker = new FileOpenPicker
        {
            ViewMode = PickerViewMode.List,
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };
        picker.FileTypeFilter.Add("*"); // Allow all files

        InitializeWithWindow.Initialize(picker, _mainWindowHandle);

        return await picker.PickMultipleFilesAsync() ?? new List<StorageFile>();
    }

    public async Task<StorageFolder?> PickFolderAsync()
    {
        var picker = new FolderPicker
        {
            ViewMode = PickerViewMode.List,
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };

        InitializeWithWindow.Initialize(picker, _mainWindowHandle);

        return await picker.PickSingleFolderAsync();
    }
}
