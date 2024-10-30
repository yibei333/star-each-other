using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarEachOther.Framework;

namespace StarEachOther.Pages;

public partial class SigninView : UserControl
{
    public SigninView()
    {
        InitializeComponent();
        DataContext = new SigninViewModel();
    }
}

public partial class SigninViewModel : ViewModelBase
{
    [ObservableProperty]
    string? message;

    [RelayCommand]
    public void Auth()
    {
        Message = "登录中,请稍后...";
        App.CurrentInstance.CoreApp.GetAuthUrl(async authUri =>
        {
            await App.CurrentInstance.Launcher.LaunchUriAsync(authUri);
        });
    }
}
