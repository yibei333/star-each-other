using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Octokit;
using StarEachOther.Core;
using StarEachOther.Framework;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace StarEachOther.Pages;

public partial class MyRepositoryView : UserControl
{
    public MyRepositoryView()
    {
        InitializeComponent();
        DataContext = new MyRepositoryViewModel();
    }
}

public partial class MyRepositoryViewModel : ObservableObject
{
    public MyRepositoryViewModel()
    {
        var data =
        (
            from a in HomeView.MyRepoList
            join b in HomeView.AllRepoList on a.HtmlUrl equals b into bb
            from b in bb.DefaultIfEmpty()
            orderby b is null
            select new MyRepositoryItemViewModel(a) { WaitAdd = b is null }
        ).ToList();
        Repo = new ObservableCollection<MyRepositoryItemViewModel>(data);
        if (data.Count <= 0) ShowContent = true;
    }

    [ObservableProperty]
    bool showContent;

    public ObservableCollection<MyRepositoryItemViewModel> Repo { get; }
}

public partial class MyRepositoryItemViewModel : ObservableObject
{
    public MyRepositoryItemViewModel(Repository repository)
    {
        Url = repository.HtmlUrl;
        StarCount = repository.StargazersCount;
    }

    [ObservableProperty]
    string url;

    [ObservableProperty]
    int starCount;

    [ObservableProperty]
    bool waitAdd;

    [RelayCommand]
    public async Task Add()
    {
        var url = string.Format(Config.NewIssueUrl, "加入", Url);
        await App.CurrentInstance.Launcher.LaunchUriAsync(new System.Uri(url));
    }

    [RelayCommand]
    public async Task Remove()
    {
        var url = string.Format(Config.NewIssueUrl, "退出", Url);
        await App.CurrentInstance.Launcher.LaunchUriAsync(new System.Uri(url));
    }
}