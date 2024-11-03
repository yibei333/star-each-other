using Avalonia.Controls;
using StarEachOther.Framework;
using System.Collections.ObjectModel;
using System.Linq;

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
        var data = (from a in HomeView.AllRepoList join b in HomeView.MyRepoList on a equals b.HtmlUrl select a).ToList();
        Repo = new ObservableCollection<string>(data);
    }

    public ObservableCollection<string> Repo { get; }
}