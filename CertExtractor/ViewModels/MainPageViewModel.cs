using CertExtractor.Commands;
using CertExtractor.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Search;
using Microsoft.UI.Dispatching;
using System.IO;

namespace CertExtractor.ViewModels;

public class MainPageViewModel : ViewModelBase
{
    // Services
    private readonly FileDialogService _fileDialogService;
    private readonly CertificateExtractorService _certificateExtractorService;
    private readonly DispatcherQueue _dispatcherQueue;

    // Backing Fields
    private string _outputDirectory = string.Empty;
    private bool _includeSubfolders = true;
    private bool _isBusy;
    private double _progressValue;
    private string _statusText = string.Empty;

    // Properties
    public string OutputDirectory
    {
        get => _outputDirectory;
        set => SetProperty(ref _outputDirectory, value);
    }

    public bool IncludeSubfolders
    {
        get => _includeSubfolders;
        set => SetProperty(ref _includeSubfolders, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public double ProgressValue
    {
        get => _progressValue;
        set => SetProperty(ref _progressValue, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }
    public ObservableCollection<string> LogMessages { get; } = new();

    // Commands
    public ICommand SelectFilesCommand { get; }
    public ICommand SelectFolderCommand { get; }
    public ICommand SelectOutputDirectoryCommand { get; }
    public ICommand ProcessDroppedItemsCommand { get; }

    public MainPageViewModel(FileDialogService fileDialogService, CertificateExtractorService certificateExtractorService)
    {
        _fileDialogService = fileDialogService;
        _certificateExtractorService = certificateExtractorService;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        // Initialize commands
        SelectFilesCommand = new RelayCommand(async () => await SelectAndProcessFilesAsync());
        SelectFolderCommand = new RelayCommand(async () => await SelectAndProcessFolderAsync());
        SelectOutputDirectoryCommand = new RelayCommand(async () => await SelectOutputDirectoryAsync());
        ProcessDroppedItemsCommand = new RelayCommand<object>(async (items) => await ProcessDroppedItemsAsync(items as IReadOnlyList<IStorageItem>));

        SetDefaultOutputDirectory();
    }

    private void SetDefaultOutputDirectory()
    {
        OutputDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
    }

    private async Task SelectAndProcessFilesAsync()
    {
        var files = await _fileDialogService.PickMultipleFilesAsync();
        if (files.Any()) { await ProcessFilesAsync(files); }
    }

    private async Task SelectAndProcessFolderAsync()
    {
        var folder = await _fileDialogService.PickFolderAsync();
        if (folder != null)
        {
            var files = await GetFilesFromFolderAsync(folder);
            await ProcessFilesAsync(files);
        }
    }

    private async Task ProcessDroppedItemsAsync(IReadOnlyList<IStorageItem>? items)
    {
        if (items == null || !items.Any()) return;

        var filesToProcess = new List<StorageFile>();
        foreach (var item in items)
        {
            if (item is StorageFile file)
            {
                filesToProcess.Add(file);
            }
            else if (item is StorageFolder folder)
            {
                filesToProcess.AddRange(await GetFilesFromFolderAsync(folder));
            }
        }
        await ProcessFilesAsync(filesToProcess);
    }

    private async Task<List<StorageFile>> GetFilesFromFolderAsync(StorageFolder folder)
    {
        var queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, new[] { "*" })
        {
            FolderDepth = IncludeSubfolders ? FolderDepth.Deep : FolderDepth.Shallow
        };
        var query = folder.CreateFileQueryWithOptions(queryOptions);
        var files = await query.GetFilesAsync();
        return files.ToList();
    }

    private async Task SelectOutputDirectoryAsync()
    {
        var folder = await _fileDialogService.PickFolderAsync();
        if (folder != null)
        {
            OutputDirectory = folder.Path;
        }
    }

    private async Task ProcessFilesAsync(IReadOnlyList<IStorageFile> files)
    {
        if (IsBusy) return;
        if (string.IsNullOrWhiteSpace(OutputDirectory))
        {
            Log("Bitte zuerst ein Ausgabeverzeichnis auswählen.");
            return;
        }

        IsBusy = true;
        LogMessages.Clear();
        Log($"Verarbeite {files.Count} Datei(en)...");

        StorageFolder outputFolder = null;
        try 
        {
            outputFolder = await StorageFolder.GetFolderFromPathAsync(OutputDirectory);
        }
        catch (FileNotFoundException ex)
        {
            Log($"FEHLER: Ausgabeverzeichnis '{OutputDirectory}' nicht gefunden.");
            IsBusy = false;
            return;
        }
        catch (UnauthorizedAccessException ex)
        {
            Log($"FEHLER: Zugriff auf Ausgabeverzeichnis '{OutputDirectory}' verweigert.");
            IsBusy = false;
            return;
        }
        catch (Exception ex)
        {
            Log($"FEHLER: {ex.Message}");
            IsBusy = false;
            return;
        }

        double currentFile = 0;
        var totalFiles = files.Count;

        foreach (var file in files)
        {
            currentFile++;
            UpdateProgress(currentFile / totalFiles * 100, $"Extrahiere aus: {file.Name}");

            var extractedCerts = _certificateExtractorService.ExtractCertificatesFromFile(file.Path);

            if (extractedCerts.Any())
            {
                var saveMessages = await _certificateExtractorService.SaveCertificatesAsync(extractedCerts, outputFolder);
                foreach (var msg in saveMessages) Log(msg);
            }
            else
            {
                Log($"INFO: Keine Zertifikate in '{file.Name}' gefunden (möglicherweise nicht signiert).");
            }
        }

        UpdateProgress(100, "Verarbeitung abgeschlossen.");
        await Task.Delay(1500); // Give user time to read final status
        IsBusy = false;
        UpdateProgress(0, string.Empty);
    }

    private void UpdateProgress(double value, string text)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            ProgressValue = value;
            StatusText = text;
        });
    }

    private void Log(string message)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            LogMessages.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
        });
    }


}