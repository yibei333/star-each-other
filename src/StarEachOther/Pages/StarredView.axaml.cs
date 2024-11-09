using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Octokit;
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

public partial class StarredViewModel : ObservableObject
{
    public StarredViewModel()
    {
        var data = (from a in HomeView.StarredRepoList orderby a.HtmlUrl select new StarredItemViewModel(a)).ToList();
        Repo = new ObservableCollection<StarredItemViewModel>(data);
        if (data.Count <= 0) ShowContent = true;
    }

    [ObservableProperty]
    bool showContent;

    public ObservableCollection<StarredItemViewModel> Repo { get; }
}

public partial class StarredItemViewModel : ObservableObject
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