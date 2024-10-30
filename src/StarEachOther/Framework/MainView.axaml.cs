using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarEachOther.Pages;
using System;
using System.Threading.Tasks;

namespace StarEachOther.Framework;

public partial class MainView : UserControl
{
    public MainViewModel ViewModel { get; set; }

    public MainView()
    {
        InitializeComponent();
        ViewModel = new MainViewModel();
        DataContext = ViewModel;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        ViewModel.Initialize().ContinueWith((s) =>
        {
            if (ViewModel.Success) Nav<HomeView>();
        });
    }

    public void Nav<TView>() where TView : class, new()
    {
        Dispatcher.UIThread.Invoke(() => ViewModel.View = new TView());
    }

    public void Refresh()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var type = ViewModel.View?.GetType();
            if (type != null)
            {
                ViewModel.View = Activator.CreateInstance(type);
            }
        });
    }
}

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    object? view;

    [ObservableProperty]
    bool showRetry;

    [ObservableProperty]
    bool success;

    [ObservableProperty]
    string? message = "初始化...";

    [RelayCommand]
    public async Task Initialize()
    {
        try
        {
            Message = "初始化...";
            ShowRetry = false;
            await App.CurrentInstance.CoreApp.ApiWrapper.Initialize();
            Success = true;
        }
        catch (Exception ex)
        {
            Message = ex.Message;
            ShowRetry = true;
        }
    }
}
