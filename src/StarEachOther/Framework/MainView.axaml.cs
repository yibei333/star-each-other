using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using StarEachOther.Pages;
using System;

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
        Nav<HomeView>();
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
}
