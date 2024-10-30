using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using StarEachOther.Framework;
using System.Linq;

namespace StarEachOther.Pages;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
        DataContext = new HomeViewModel();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        (DataContext as HomeViewModel)?.GetRepositories();
    }
}

public partial class HomeViewModel : ViewModelBase
{
    public void GetRepositories()
    {
        if (string.IsNullOrWhiteSpace(App.CurrentInstance.CoreApp.ApiWrapper.GetToken()))
        {
            App.CurrentInstance.MainView.Nav<SigninView>();
            return;
        }
        App.CurrentInstance.CoreApp.GetRepositories(repositories =>
        {
            Greeting = string.Join(";", repositories.Select(x => x.Name));
        });
    }

    [ObservableProperty]
    private string _greeting = "Home";
}