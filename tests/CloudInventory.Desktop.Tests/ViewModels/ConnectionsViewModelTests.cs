using CloudInventory.Application.Connections;
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
        var viewModel = new ConnectionsViewModel(catalog);

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
        var viewModel = new ConnectionsViewModel(catalog);

        await viewModel.EnsureLoadedAsync();
        await viewModel.EnsureLoadedAsync();

        Assert.Equal(1, catalog.CallCount);
        Assert.True(viewModel.IsEmpty);
    }

    [Fact]
    public async Task RefreshAsyncShowsSafeErrorWhenCatalogFails()
    {
        var catalog = new StubAwsProfileCatalog(new InvalidOperationException("Sensitive detail"));
        var viewModel = new ConnectionsViewModel(catalog);

        await viewModel.RefreshAsync();

        Assert.True(viewModel.HasError);
        Assert.DoesNotContain("Sensitive detail", viewModel.ErrorMessage);
        Assert.Empty(viewModel.Profiles);
        Assert.False(viewModel.IsLoading);
    }

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
}
