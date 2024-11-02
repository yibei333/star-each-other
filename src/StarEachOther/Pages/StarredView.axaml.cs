using Avalonia.Controls;
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

public class StarredViewModel : ViewModelBase
{
    public StarredViewModel()
    {
        var data = (from a in HomeView.AllRepoList join b in HomeView.StarredRepoList on a equals b.HtmlUrl select a).ToList();
        Repo = new ObservableCollection<string>(data);
    }

    public ObservableCollection<string> Repo { get; }
}