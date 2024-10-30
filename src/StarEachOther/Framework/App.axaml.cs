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
    public CoreApp CoreApp { get; private set; } = null!;
    public MainView MainView { get; private set; } = null!;
    public ILauncher Launcher => TopLevel.GetTopLevel(MainView)!.Launcher;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        CoreApp = new CoreApp(RequestError, RequireSignin, SigninResult);
        CoreApp.Start();
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
            desktop.Exit += (s, e) => CoreApp?.Stop();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = MainView;
            singleViewPlatform.MainView.Unloaded += (s, e) => CoreApp?.Stop();
        }

        base.OnFrameworkInitializationCompleted();
    }

    async Task RequestError(Exception e)
    {
        var box = MessageBoxManager.GetMessageBoxStandard("错误", e.Message, ButtonEnum.Ok);
        await box.ShowAsync();
        MainView.Refresh();
    }

    async Task RequireSignin(Octokit.AuthorizationException e)
    {
        await Task.Yield();
        MainView.Nav<SigninView>();
    }

    void SigninResult(bool success)
    {
        if (success) MainView.Nav<HomeView>();
        else MainView.Refresh();
    }
}