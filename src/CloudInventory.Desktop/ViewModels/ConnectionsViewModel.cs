using CloudInventory.Application.Connections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CloudInventory.Desktop.ViewModels;

public partial class ConnectionsViewModel : ViewModelBase
{
    private readonly IAwsProfileCatalog _profileCatalog;
    private bool _hasLoaded;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasProfiles))]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    public partial IReadOnlyList<ConnectionProfileViewModel> Profiles { get; private set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    public partial bool IsLoading { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    public partial string? ErrorMessage { get; private set; }

    public bool HasProfiles => Profiles.Count > 0;

    public bool HasError => ErrorMessage is not null;

    public bool IsEmpty => !IsLoading && !HasError && !HasProfiles;

    public ConnectionsViewModel(IAwsProfileCatalog profileCatalog)
    {
        _profileCatalog = profileCatalog;
    }

    public Task EnsureLoadedAsync() =>
        _hasLoaded ? Task.CompletedTask : RefreshAsync();

    [RelayCommand]
    public async Task RefreshAsync()
    {
        if (IsLoading)
        {
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var profiles = await _profileCatalog.ListAsync();

            Profiles = profiles
                .Select(ConnectionProfileViewModel.From)
                .ToArray();
            _hasLoaded = true;
        }
        catch (Exception)
        {
            Profiles = [];
            ErrorMessage = "AWS profiles could not be loaded. Check your shared AWS configuration and try again.";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public sealed record ConnectionProfileViewModel(
    string Name,
    AwsAuthenticationKind AuthenticationKind,
    string AuthenticationLabel,
    string Region,
    string Account,
    string Role)
{
    public static ConnectionProfileViewModel From(AwsProfileSummary profile) =>
        new(
            profile.Name,
            profile.AuthenticationKind,
            GetAuthenticationLabel(profile.AuthenticationKind),
            profile.Region ?? "Not configured",
            profile.AccountId ?? "Not available",
            profile.RoleName ?? "Not applicable");

    private static string GetAuthenticationLabel(AwsAuthenticationKind authenticationKind) =>
        authenticationKind switch
        {
            AwsAuthenticationKind.Sso => "IAM Identity Center",
            AwsAuthenticationKind.AssumeRole => "Assume role",
            AwsAuthenticationKind.CredentialProcess => "Credential process",
            AwsAuthenticationKind.WebIdentity => "Web identity",
            AwsAuthenticationKind.StaticCredentials => "Named credentials",
            _ => "Unknown",
        };
}
