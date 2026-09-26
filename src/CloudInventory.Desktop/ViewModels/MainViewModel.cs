using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CloudInventory.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public IReadOnlyList<NavigationItemViewModel> NavigationItems { get; } =
    [
        new("Overview", "Workspace summary"),
        new("EC2", "Compute"),
        new("Kubernetes", "Compute"),
        new("RDS", "Databases"),
        new("ElastiCache", "Databases"),
        new("S3", "Storage"),
        new("CloudWatch Logs", "Observability"),
        new("Connections", "Configuration"),
        new("Settings", "Configuration"),
    ];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentPageTitle))]
    [NotifyPropertyChangedFor(nameof(CurrentPageDescription))]
    public partial NavigationItemViewModel? SelectedNavigationItem { get; set; }

    [ObservableProperty]
    public partial string LastRefreshed { get; set; } = "Not refreshed";

    public string CurrentPageTitle => SelectedNavigationItem?.Label ?? "Overview";

    public string CurrentPageDescription =>
        SelectedNavigationItem?.Description ?? "Workspace summary";

    public MainViewModel()
    {
        SelectedNavigationItem = NavigationItems[0];
    }

    [RelayCommand]
    private void Refresh()
    {
        LastRefreshed = $"Refreshed {DateTimeOffset.Now:HH:mm}";
    }
}

public sealed record NavigationItemViewModel(string Label, string Description);
