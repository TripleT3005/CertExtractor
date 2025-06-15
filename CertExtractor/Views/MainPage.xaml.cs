using CertExtractor.ViewModels;
using Microsoft.UI.Xaml.Controls;
using CertExtractor.Services;
using Microsoft.UI.Xaml;
using System;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using CertExtractor.Converters;

namespace CertExtractor.Views;

public sealed partial class MainPage : Page
{
    public MainPageViewModel ViewModel { get; }

    public MainPage()
    {
        this.InitializeComponent();

        var fileDialogService = new FileDialogService(App.MainWindow);
        var certificateExtractorService = new CertificateExtractorService();

        ViewModel = new MainPageViewModel(fileDialogService, certificateExtractorService);

        this.DataContext = ViewModel;

        this.Resources.Add("BooleanToVisibilityConverter", new BooleanToVisibilityConverter());
        this.Resources.Add("InvertedBooleanConverter", new InvertedBooleanConverter());
    }

    private void DropArea_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
        e.DragUIOverride.Caption = "Dateien/Ordner hier ablegen";
        e.DragUIOverride.IsCaptionVisible = true;
        e.DragUIOverride.IsContentVisible = true;
    }

    private async void DropArea_Drop(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await e.DataView.GetStorageItemsAsync();
            if (items.Any())
            {
                ViewModel.ProcessDroppedItemsCommand.Execute(items);
            }
        }
    }

}