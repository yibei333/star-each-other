using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StarEachOther.Core;
using StarEachOther.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace StarEachOther.Pages;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
        DataContext = new HomeViewModel();
    }
}

public partial class HomeViewModel : ViewModelBase
{
    readonly Dictionary<int, object> _cache = [];

    public HomeViewModel()
    {
        _cache.Add(1, new WaitForStarView());
        _cache.Add(2, new StarredView());
        _cache.Add(3, new MyRepositoryView());
        Switch(1);
        SupportAutherUrl = Config.SupportAutherUrl;
    }

    [ObservableProperty]
    object? view;

    [ObservableProperty]
    int index = 0;

    [ObservableProperty]
    string supportAutherUrl;

    void Switch(int index)
    {
        Index = index;
        if (_cache.TryGetValue(index, out object? value))
        {
            View = value;
        }
    }

    [RelayCommand]
    public void SwitchWaitForStarView() => Switch(1);

    [RelayCommand]
    public void SwitchStarredView() => Switch(2);

    [RelayCommand]
    public void SwitchMyRepositoryView() => Switch(3);

    [RelayCommand]
    public void Refresh()
    {
        var type = View?.GetType();
        if (type is null) return;
        var instance = Activator.CreateInstance(type);
        if (instance is null) return;
        _cache[Index] = instance;
        View = instance;
    }
}

public class ButtonActiveConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null || parameter is null) return false;
        return value.ToString() == parameter.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}