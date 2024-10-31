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
    public async void GetRepositories()
    {
        await App.CurrentInstance.Client.Request(async client =>
         {
             var repositories = await client.Repository.GetAllForCurrent();
             Message = string.Join(";", repositories.Select(x => x.Name));
         });
    }

    [ObservableProperty]
    private string message = "Home";
}