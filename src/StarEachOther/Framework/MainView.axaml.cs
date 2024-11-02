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
        _ = ViewModel.Initialize();
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

    public void SetLoadingState(bool isLoading, string? loadingText)
    {
        ViewModel.Loading = isLoading;
        ViewModel.LoadingText = loadingText;
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
    string? loadingText;

    [ObservableProperty]
    bool loading;

    [RelayCommand]
    public async Task Initialize()
    {
        try
        {
            Loading = true;
            LoadingText = "初始化...";
            ShowRetry = false;
            await App.CurrentInstance.Client.Initialize();
            Success = true;
            App.CurrentInstance.MainView.Nav<HomeView>();
            LoadingText = string.Empty;
            Loading = false;
        }
        catch (Exception ex)
        {
            Loading = false;
            LoadingText = ex.Message;
            ShowRetry = true;
        }
    }
}
