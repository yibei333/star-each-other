using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Octokit;
using StarEachOther.Core;
using StarEachOther.Framework;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace StarEachOther.Pages;

public partial class WaitForStarView : UserControl
{
    public WaitForStarView()
    {
        InitializeComponent();
        DataContext = new WaitForStarViewModel();
    }
}

public partial class WaitForStarViewModel : ViewModelBase
{
    public WaitForStarViewModel()
    {
        var data = (
            from a in HomeView.AllRepoList
            join b in HomeView.StarredRepoList on a equals b.HtmlUrl into bb
            from b in bb.DefaultIfEmpty()
            where b is null
            select new WaitForStarItemViewModel(a, this)
        ).ToList();
        Repo = new ObservableCollection<WaitForStarItemViewModel>(data);
        if (data.Count <= 0) ShowContent = true;
    }

    [ObservableProperty]
    bool showContent;

    public ObservableCollection<WaitForStarItemViewModel> Repo { get; }
}

public partial class WaitForStarItemViewModel : ViewModelBase
{
    public WaitForStarItemViewModel(string url, WaitForStarViewModel parent)
    {
        Url = url;
        Parent = parent;
    }

    [ObservableProperty]
    string url;

    public WaitForStarViewModel Parent { get; }

    [RelayCommand]
    public async Task Star()
    {
        var repoInfo = Url.GetUserAndRepoNameByUrl();
        if (!repoInfo.Item1)
        {
            await App.Alert($"解析url({Url})失败,请联系开发者");
            return;
        }

        var result = await App.CurrentInstance.Client.Request(async client =>
        {
            var url = $"{GitHubClient.GitHubApiUrl}user/starred/{repoInfo.Item3}/{repoInfo.Item4}";
            await client.Connection.Put<int>(new Uri(url), null);
        });

        if (result)
        {
            Parent.Repo.Remove(this);
            Update(repoInfo.Item3, repoInfo.Item4);
        }
    }

    async void Update(string user, string repoName)
    {
        await App.CurrentInstance.Client.Request(async client =>
        {
            var repo = await client.Repository.Get(user, repoName);
            if (repo is not null)
            {
                HomeView.StarredRepoList.Add(repo);

                var myRepo = HomeView.MyRepoList.FirstOrDefault(x => x.HtmlUrl == Url);
                if (myRepo is not null)
                {
                    HomeView.MyRepoList.Remove(myRepo);
                    HomeView.MyRepoList.Add(repo);
                }
            }
        });
    }
}