using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarEachOther.Framework;
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
            select new RepoItemViewModel { Url = a }
        ).ToList();
        Repo = new ObservableCollection<RepoItemViewModel>(data);
    }

    public ObservableCollection<RepoItemViewModel> Repo { get; }
}

public partial class RepoItemViewModel : ViewModelBase
{
    [ObservableProperty]
    string? url;

    [RelayCommand]
    public async Task Star()
    {
        await Task.Delay(5000);
        await App.Alert(Url ?? string.Empty);
    }
}