using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CloudInventory.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly ConnectionsViewModel _connectionsViewModel;
    private readonly NavigationItemViewModel _connectionsNavigationItem;

    public IReadOnlyList<NavigationItemViewModel> NavigationItems { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentPageTitle))]
    [NotifyPropertyChangedFor(nameof(CurrentPageDescription))]
    public partial NavigationItemViewModel? SelectedNavigationItem { get; set; }

    [ObservableProperty]
    public partial string LastRefreshed { get; set; } = "Not refreshed";

    [ObservableProperty]
    public partial ViewModelBase? CurrentPage { get; private set; }

    public string CurrentPageTitle => SelectedNavigationItem?.Label ?? "Overview";

    public string CurrentPageDescription =>
        SelectedNavigationItem?.Description ?? "Workspace summary";

    public MainViewModel(
        OverviewViewModel overviewViewModel,
        ConnectionsViewModel connectionsViewModel)
    {
        _connectionsViewModel = connectionsViewModel;
        _connectionsNavigationItem = new(
            "Connections",
            "Configuration",
            connectionsViewModel);

        NavigationItems =
        [
            new("Overview", "Workspace summary", overviewViewModel),
            new("EC2", "Compute", overviewViewModel),
            new("Kubernetes", "Compute", overviewViewModel),
            new("RDS", "Databases", overviewViewModel),
            new("ElastiCache", "Databases", overviewViewModel),
            new("S3", "Storage", overviewViewModel),
            new("CloudWatch Logs", "Observability", overviewViewModel),
            _connectionsNavigationItem,
            new("Settings", "Configuration", overviewViewModel),
        ];

        SelectedNavigationItem = NavigationItems[0];
    }

    partial void OnSelectedNavigationItemChanged(NavigationItemViewModel? value)
    {
        CurrentPage = value?.Content;

        if (ReferenceEquals(value, _connectionsNavigationItem))
        {
            _ = _connectionsViewModel.EnsureLoadedAsync();
        }
    }

    [RelayCommand]
    private void Refresh()
    {
        LastRefreshed = $"Refreshed {DateTimeOffset.Now:HH:mm}";
    }

    [RelayCommand]
    private void ManageConnections()
    {
        SelectedNavigationItem = _connectionsNavigationItem;
    }
}

public sealed record NavigationItemViewModel(
    string Label,
    string Description,
    ViewModelBase Content);
