using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Octokit;
using SharpDevLib;
using StarEachOther.Core;
using StarEachOther.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace StarEachOther.Pages;

public partial class HomeView : UserControl
{
    public static List<string> AllRepoList { get; private set; } = [];
    public static List<StarredRepositoryModel> StarredRepoList { get; private set; } = [];
    public static List<Repository> MyRepoList { get; private set; } = [];

    public HomeView()
    {
        InitializeComponent();
        DataContext = new HomeViewModel();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _ = (DataContext as HomeViewModel)!.Refresh();
    }

    public static async Task<bool> SetRepo()
    {
        var result = false;

        //all registered repo
        var text = await HttpExtension.GetText(Config.RepoListUrl);
        if (!text.Success)
        {
            await App.Alert(text.Data);
            return result;
        }
        AllRepoList = text.Data.Split(new string[] { "\r", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Distinct().ToList();

        //starred repo
        result = await App.CurrentInstance.Client.Request(async client =>
        {
            var user = await client.User.Current();
            var model = await new ApiConnection(client.Connection).GetAll<StarredRepositoryModel>(new Uri($"{GitHubClient.GitHubApiUrl}users/{user.Login}/starred"));
            StarredRepoList = model.ToList();
        });
        if (!result) return false;

        //my repo
        result = await App.CurrentInstance.Client.Request(async client =>
        {
            MyRepoList = (await client.Repository.GetAllForCurrent(new RepositoryRequest { Type = RepositoryType.Public })).ToList();
        });
        return result;
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
            App.CurrentInstance.MainView.SetLoadingState(false, null);
            Switch(Index);
        }
        else
        {
            ShowRetry = true;
            App.CurrentInstance.MainView.SetLoadingState(false, null);
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

public class StarredRepositoryModel
{
    public string? HtmlUrl { get; set; }
}