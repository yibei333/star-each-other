using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Octokit;
using StarEachOther.Framework;
using System.Collections.ObjectModel;
using System.Linq;

namespace StarEachOther.Pages;

public partial class StarredView : UserControl
{
    public StarredView()
    {
        InitializeComponent();
        DataContext = new StarredViewModel();
    }
}

public partial class StarredViewModel : ViewModelBase
{
    public StarredViewModel()
    {
        var data = (from a in HomeView.AllRepoList join b in HomeView.StarredRepoList on a equals b.HtmlUrl select new StarredItemViewModel(b)).ToList();
        Repo = new ObservableCollection<StarredItemViewModel>(data);
    }

    public ObservableCollection<StarredItemViewModel> Repo { get; }
}

public partial class StarredItemViewModel : ViewModelBase
{
    public StarredItemViewModel(Repository repository)
    {
        Url = repository.HtmlUrl;
        StarCount = repository.StargazersCount;
    }

    [ObservableProperty]
    string url;

    [ObservableProperty]
    int starCount;
}