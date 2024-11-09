using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarEachOther.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
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
        Nav<InitialView>();
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

    public void SetLoadingState(bool isLoading, string? loadingText = null)
    {
        ViewModel.IsLoading = isLoading;
        ViewModel.LoadingText = loadingText;
    }
}

public partial class MainViewModel : ObservableObject
{
    public MainViewModel()
    {
        Messages = [];
        MessageTasks = [];
        Messages.CollectionChanged += (s, e) =>
        {
            ShowMessage = Messages.Count > 0;
        };
    }

    [ObservableProperty]
    object? view;

    [ObservableProperty]
    bool isLoading;

    [ObservableProperty]
    string? loadingText;

    [ObservableProperty]
    bool showMessage;

    public ObservableCollection<MessageModel> Messages { get; }

    internal Queue<MessageModel> MessageTasks { get; }

    public void AddMessage(string message, string? taskButtonText = null, Task? task = null)
    {
        MessageTasks.Enqueue(new MessageModel(this) { Message = message, TaskButtonText = taskButtonText, Task = task, ShowButton = task is not null });
        HandleMessage();
    }

    internal void HandleMessage()
    {
        if (Messages.Count >= 3) return;
        if (MessageTasks.Count == 0) return;
        MessageTasks.Dequeue().Attach();
    }
}

public partial class MessageModel(MainViewModel parent) : ObservableObject
{
    int CloseSeconds { get; set; } = 5;
    [ObservableProperty]
    string? message;
    [ObservableProperty]
    string? taskButtonText;
    [ObservableProperty]
    bool showButton;
    public Task? Task { get; set; }
    MainViewModel Parent { get; } = parent;

    internal async void Attach()
    {
        Parent.Messages.Add(this);
        if (Task is null)
        {
            await Task.Delay(CloseSeconds * 1000);
            Close();
        }
    }

    [RelayCommand]
    public void Close()
    {
        try
        {
            Parent.Messages.Remove(this);
        }
        catch { }
        Parent.HandleMessage();
    }

    [RelayCommand]
    public async Task Process()
    {
        if (Task is null) return;
        Task.Start();
        await this.Task.WaitAsync(CancellationToken.None);
        Close();
    }
}