using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarEachOther.Framework;
using System.Threading.Tasks;

namespace StarEachOther.Pages;

public partial class SigninView : UserControl
{
    public SigninView()
    {
        InitializeComponent();
        DataContext = new SigninViewModel();
    }
}

public partial class SigninViewModel : ObservableObject
{
    [ObservableProperty]
    string? message;

    [RelayCommand]
    public async Task Auth()
    {
        Message = "登录中,请稍后...";
        var authUri = App.CurrentInstance.Client.GetAuthUrl();
        await App.CurrentInstance.Launcher.LaunchUriAsync(authUri);
    }
}
