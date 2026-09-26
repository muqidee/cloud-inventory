using CloudInventory.Application.Connections;
using CloudInventory.Desktop.Services;
using CloudInventory.Desktop.ViewModels;

namespace CloudInventory.Desktop.Tests.ViewModels;

public sealed class ConnectionsViewModelTests
{
    [Fact]
    public async Task RefreshAsyncMapsDiscoveredProfiles()
    {
        var catalog = new StubAwsProfileCatalog(
        [
            new AwsProfileSummary(
                "production",
                AwsAuthenticationKind.Sso,
                "eu-west-1",
                "123456789012",
                "ReadOnly",
                "https://example.awsapps.com/start"),
        ]);
        var viewModel = CreateViewModel(catalog);

        await viewModel.RefreshAsync();

        var profile = Assert.Single(viewModel.Profiles);
        Assert.Equal("production", profile.Name);
        Assert.Equal("IAM Identity Center", profile.AuthenticationLabel);
        Assert.Equal("eu-west-1", profile.Region);
        Assert.Equal("123456789012", profile.Account);
        Assert.Equal("ReadOnly", profile.Role);
        Assert.True(viewModel.HasProfiles);
        Assert.False(viewModel.IsEmpty);
        Assert.False(viewModel.HasError);
    }

    [Fact]
    public async Task EnsureLoadedAsyncLoadsCatalogOnlyOnce()
    {
        var catalog = new StubAwsProfileCatalog([]);
        var viewModel = CreateViewModel(catalog);

        await viewModel.EnsureLoadedAsync();
        await viewModel.EnsureLoadedAsync();

        Assert.Equal(1, catalog.CallCount);
        Assert.True(viewModel.IsEmpty);
    }

    [Fact]
    public async Task RefreshAsyncShowsSafeErrorWhenCatalogFails()
    {
        var catalog = new StubAwsProfileCatalog(new InvalidOperationException("Sensitive detail"));
        var viewModel = CreateViewModel(catalog);

        await viewModel.RefreshAsync();

        Assert.True(viewModel.HasError);
        Assert.DoesNotContain("Sensitive detail", viewModel.ErrorMessage);
        Assert.Empty(viewModel.Profiles);
        Assert.False(viewModel.IsLoading);
    }

    [Fact]
    public async Task ConnectCommandStoresVerifiedIdentity()
    {
        var identity = new AwsConnectionIdentity(
            "production",
            "123456789012",
            "arn:aws:sts::123456789012:assumed-role/ReadOnly/user",
            "user-id",
            "eu-west-1");
        var service = new StubAwsConnectionService(identity);
        var session = new ConnectionSessionViewModel();
        var launcher = new StubUriLauncher();
        var viewModel = new ConnectionsViewModel(
            new StubAwsProfileCatalog([]),
            service,
            session,
            launcher);
        var profile = CreateProfile();

        await viewModel.ConnectCommand.ExecuteAsync(profile);

        Assert.True(session.IsConnected);
        Assert.Equal(identity, session.Identity);
        Assert.Equal("production", session.ConnectionLabel);
        Assert.Equal("123456789012 / eu-west-1", session.SessionLabel);
        Assert.False(viewModel.HasConnectionError);
    }

    [Fact]
    public async Task ConnectCommandOpensSsoVerificationPage()
    {
        var service = new StubAwsConnectionService(
            new AwsConnectionIdentity(
                "production",
                "123456789012",
                "arn:aws:iam::123456789012:role/ReadOnly",
                "user-id",
                "eu-west-1"),
            new AwsSignInInstruction(
                new Uri("https://device.sso.example/verify"),
                "ABCD-EFGH"));
        var launcher = new StubUriLauncher();
        var viewModel = new ConnectionsViewModel(
            new StubAwsProfileCatalog([]),
            service,
            new ConnectionSessionViewModel(),
            launcher);

        await viewModel.ConnectCommand.ExecuteAsync(CreateProfile());

        Assert.Equal("https://device.sso.example/verify", launcher.OpenedUri?.AbsoluteUri);
    }

    [Fact]
    public async Task ConnectCommandShowsSafeErrorWhenConnectionFails()
    {
        var service = new StubAwsConnectionService(
            new InvalidOperationException("Sensitive detail"));
        var session = new ConnectionSessionViewModel();
        var viewModel = new ConnectionsViewModel(
            new StubAwsProfileCatalog([]),
            service,
            session,
            new StubUriLauncher());

        await viewModel.ConnectCommand.ExecuteAsync(CreateProfile());

        Assert.True(viewModel.HasConnectionError);
        Assert.DoesNotContain("Sensitive detail", viewModel.ConnectionErrorMessage);
        Assert.False(session.IsConnected);
        Assert.False(viewModel.IsConnecting);
    }

    private static ConnectionsViewModel CreateViewModel(IAwsProfileCatalog catalog) =>
        new(
            catalog,
            new StubAwsConnectionService(
                new AwsConnectionIdentity(
                    "production",
                    "123456789012",
                    "arn:aws:iam::123456789012:role/ReadOnly",
                    "user-id",
                    "eu-west-1")),
            new ConnectionSessionViewModel(),
            new StubUriLauncher());

    private static ConnectionProfileViewModel CreateProfile() =>
        ConnectionProfileViewModel.From(
            new AwsProfileSummary(
                "production",
                AwsAuthenticationKind.Sso,
                "eu-west-1",
                "123456789012",
                "ReadOnly",
                "https://example.awsapps.com/start"));

    private sealed class StubAwsProfileCatalog : IAwsProfileCatalog
    {
        private readonly IReadOnlyList<AwsProfileSummary> _profiles;
        private readonly Exception? _exception;

        public StubAwsProfileCatalog(IReadOnlyList<AwsProfileSummary> profiles)
        {
            _profiles = profiles;
        }

        public StubAwsProfileCatalog(Exception exception)
        {
            _profiles = [];
            _exception = exception;
        }

        public int CallCount { get; private set; }

        public Task<IReadOnlyList<AwsProfileSummary>> ListAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;

            return _exception is null
                ? Task.FromResult(_profiles)
                : Task.FromException<IReadOnlyList<AwsProfileSummary>>(_exception);
        }
    }

    private sealed class StubAwsConnectionService : IAwsConnectionService
    {
        private readonly AwsConnectionIdentity? _identity;
        private readonly AwsSignInInstruction? _signInInstruction;
        private readonly Exception? _exception;

        public StubAwsConnectionService(
            AwsConnectionIdentity identity,
            AwsSignInInstruction? signInInstruction = null)
        {
            _identity = identity;
            _signInInstruction = signInInstruction;
        }

        public StubAwsConnectionService(Exception exception)
        {
            _exception = exception;
        }

        public Task<AwsConnectionIdentity> ConnectAsync(
            string profileName,
            Action<AwsSignInInstruction> signInRequired,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_exception is not null)
            {
                return Task.FromException<AwsConnectionIdentity>(_exception);
            }

            if (_signInInstruction is not null)
            {
                signInRequired(_signInInstruction);
            }

            return Task.FromResult(_identity!);
        }
    }

    private sealed class StubUriLauncher : IUriLauncher
    {
        public Uri? OpenedUri { get; private set; }

        public void Open(Uri uri)
        {
            OpenedUri = uri;
        }
    }
}
