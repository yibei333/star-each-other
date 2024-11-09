using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharpDevLib;
using StarEachOther.Core;
using StarEachOther.Framework;
using System;
using System.IO;
using System.Threading.Tasks;

namespace StarEachOther.Pages;

public partial class InitialView : UserControl
{
    public InitialViewModel ViewModel { get; }

    public InitialView()
    {
        InitializeComponent();
        ViewModel = new InitialViewModel();
        DataContext = ViewModel;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _ = ViewModel.Process();
        ViewModel.ChekcUpdate();
    }
}

public partial class InitialViewModel : ObservableObject
{
    [ObservableProperty]
    bool showRetry;

    [ObservableProperty]
    string? message;

    [RelayCommand]
    public async Task Process()
    {
        try
        {
            Message = "初始化...";
            ShowRetry = false;
            await App.CurrentInstance.Client.Initialize();
            if (App.CurrentInstance.Client.IsAuthenticated) App.CurrentInstance.MainView.Nav<HomeView>();
            else App.CurrentInstance.MainView.Nav<SigninView>();
        }
        catch (Exception ex)
        {
            Message = ex.Message;
            ShowRetry = true;
        }
    }

    public async void ChekcUpdate()
    {
        try
        {
            var remoteVersion = await HttpExtension.GetText(Config.VersionUrl);
            if (remoteVersion.Success && remoteVersion.Data.NotNullOrWhiteSpace())
            {
                var assembly = this.GetType().Assembly;
                var uri = new Uri($"avares://{assembly.GetName().Name}/Assets/version.txt");
                using var stream = AssetLoader.Open(uri);
                using var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                var currentVersion = memoryStream.ToArray().Utf8Encode().Trim();
                if (currentVersion.NotNullOrWhiteSpace() && currentVersion != remoteVersion.Data)
                {
                    App.CurrentInstance.Alert($"发现新版本:{remoteVersion.Data}", "去下载", new Task(async () =>
                    {
                        await App.CurrentInstance.Launcher.LaunchUriAsync(new Uri(Config.ReleaseUrl));
                    }));
                }
            }
        }
        catch
        {
        }
    }
}
