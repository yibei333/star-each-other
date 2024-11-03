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

public partial class MyRepositoryViewModel : ViewModelBase
{
    public MyRepositoryViewModel()
    {
        var data = (from a in HomeView.AllRepoList join b in HomeView.MyRepoList on a equals b.HtmlUrl select new MyRepositoryItemViewModel(b)).ToList();
        Repo = new ObservableCollection<MyRepositoryItemViewModel>(data);
    }

    public ObservableCollection<MyRepositoryItemViewModel> Repo { get; }

    [RelayCommand]
    public async Task New()
    {
        await App.Alert("请添加一个Isuue,内容格式如下:【添加:https://github.com/your/repo】", "知道了");
        await App.CurrentInstance.Launcher.LaunchUriAsync(new System.Uri(Config.NewRepoUrl));
    }
}

public partial class MyRepositoryItemViewModel : ViewModelBase
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
}