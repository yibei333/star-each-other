using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using StarEachOther.Core;
using StarEachOther.Pages;
using System;
using System.Threading.Tasks;

namespace StarEachOther.Framework;

public partial class App : Application
{
    public static App CurrentInstance { get; private set; } = null!;
    public LocalServerWrapper Server { get; private set; } = null!;
    public GithubClientWrapper Client { get; private set; } = null!;
    public MainView MainView { get; private set; } = null!;
    public ILauncher Launcher => TopLevel.GetTopLevel(MainView)!.Launcher;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        Client = new GithubClientWrapper();
        Client.RequestException += OnRequestException;
        Client.UnAuthenticated += OnUnAuthenticated;

        Server = new LocalServerWrapper(Client);
        Server.SigninCallback += OnSigninCallback;
        Server.Start();

        CurrentInstance = this;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        MainView = new MainView();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            BindingPlugins.DataValidators.RemoveAt(0);
            desktop.MainWindow = new MainWindow
            {
                Content = MainView
            };
            desktop.Exit += (s, e) => Server?.Stop();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = MainView;
            singleViewPlatform.MainView.Unloaded += (s, e) => Server?.Stop();
        }

        base.OnFrameworkInitializationCompleted();
    }

    async Task OnRequestException(Exception e)
    {
        await Alert("错误", e.Message);
        //MainView.Refresh();
    }

    async Task OnUnAuthenticated(Octokit.AuthorizationException e)
    {
        MainView.Nav<SigninView>();
        await Task.CompletedTask;
    }

    async Task OnSigninCallback(bool success)
    {
        if (success) MainView.Nav<HomeView>();
        else MainView.Refresh();
        await Task.CompletedTask;
    }

    public static async Task Alert(string title, string text)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(title, text, ButtonEnum.Ok);
        await box.ShowAsync();
    }
}