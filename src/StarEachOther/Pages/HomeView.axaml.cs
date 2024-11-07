using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Octokit;
using SharpDevLib;
using StarEachOther.Core;
using StarEachOther.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StarEachOther.Pages;

public partial class HomeView : UserControl
{
    public HomeViewModel ViewModel { get; }
    public static List<string> AllRepoList { get; private set; } = [];
    public static List<Repository> StarredRepoList { get; private set; } = [];
    public static List<Repository> MyRepoList { get; private set; } = [];

    public HomeView()
    {
        InitializeComponent();
        ViewModel = new HomeViewModel();
        DataContext = ViewModel;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _ = ViewModel.Refresh();
        ViewModel.ChekcUpdate();
    }

    public static async Task<bool> SetRepo()
    {
        var result = await SetRegistedRepo();
        if (!result) return false;

        result = await SetStarredRepo();
        if (!result) return false;

        result = await SetMyRepo();
        if (!result) return false;

        return true;
    }

    public static async Task<bool> SetRegistedRepo()
    {
        var text = await HttpExtension.GetText(Config.RepoListUrl);
        if (!text.Success)
        {
            await App.Alert(text.Data);
            return false;
        }
        AllRepoList = text.Data.Split(new string[] { "\r", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Distinct().ToList();
        return true;
    }

    public static async Task<bool> SetStarredRepo()
    {
        return await App.CurrentInstance.Client.Request(async client =>
        {
            var user = await client.User.Current();
            var model = await new ApiConnection(client.Connection).GetAll<Repository>(new Uri($"{GitHubClient.GitHubApiUrl}users/{user.Login}/starred"));
            StarredRepoList = [.. model];
        });
    }

    public static async Task<bool> SetMyRepo()
    {
        return await App.CurrentInstance.Client.Request(async client =>
        {
            MyRepoList = [.. (await client.Repository.GetAllForCurrent(new RepositoryRequest { Type = RepositoryType.Public }))];
        });
    }
}

public partial class HomeViewModel : ViewModelBase
{
    [ObservableProperty]
    object? view;

    [ObservableProperty]
    bool showRetry;

    [ObservableProperty]
    int index = 1;

    [ObservableProperty]
    string supportAutherUrl = Config.SupportAutherUrl;

    void Switch(int index)
    {
        Index = index;
        if (index == 1)
        {
            View = new WaitForStarView();
        }
        else if (index == 2)
        {
            View = new StarredView();
        }
        else
        {
            View = new MyRepositoryView();
        }
    }

    [RelayCommand]
    public void SwitchWaitForStarView() => Switch(1);

    [RelayCommand]
    public void SwitchStarredView() => Switch(2);

    [RelayCommand]
    public void SwitchMyRepositoryView() => Switch(3);

    [RelayCommand]
    public async Task Refresh()
    {
        App.CurrentInstance.MainView.SetLoadingState(true, "加载数据");
        if (await HomeView.SetRepo())
        {
            ShowRetry = false;
            Switch(Index);
            App.CurrentInstance.MainView.SetLoadingState(false, null);
        }
        else
        {
            ShowRetry = true;
            App.CurrentInstance.MainView.SetLoadingState(false, null);
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
                    await App.Alert($"发现新版本:{remoteVersion.Data}", "去下载");
                    await App.CurrentInstance.Launcher.LaunchUriAsync(new Uri(Config.ReleaseUrl));
                }
            }
        }
        catch
        {
        }
    }
}

public class ButtonActiveConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null || parameter is null) return false;
        return value.ToString() == parameter.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}