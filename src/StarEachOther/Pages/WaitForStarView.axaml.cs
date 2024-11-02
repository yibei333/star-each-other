using Avalonia.Controls;
using StarEachOther.Framework;
using System.Collections.ObjectModel;
using System.Linq;

namespace StarEachOther.Pages;

public partial class WaitForStarView : UserControl
{
    public WaitForStarView()
    {
        InitializeComponent();
        DataContext = new WaitForStarViewModel();
    }
}

public class WaitForStarViewModel : ViewModelBase
{
    public WaitForStarViewModel()
    {
        var data = (from a in HomeView.AllRepoList join b in HomeView.StarredRepoList on a equals b.HtmlUrl into bb from b in bb.DefaultIfEmpty() where b is null select a).ToList();
        Repo = new ObservableCollection<string>(data);
    }

    public ObservableCollection<string> Repo { get; }
}