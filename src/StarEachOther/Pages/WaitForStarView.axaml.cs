using Avalonia.Controls;
using StarEachOther.Framework;
using System.Collections.ObjectModel;

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
    public ObservableCollection<string> Repo { get; } = new ObservableCollection<string>(HomeView.AllRepoList);
}