using CloudInventory.Infrastructure.Aws.Connections;

namespace CloudInventory.Infrastructure.Aws.Tests.Connections;

public sealed class AwsConnectionServiceTests : IDisposable
{
    private readonly string _profilesPath = Path.GetTempFileName();

    [Fact]
    public async Task ConnectAsyncRejectsUnknownProfile()
    {
        var service = new AwsConnectionService(_profilesPath);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ConnectAsync("missing", _ => { }));

        Assert.Contains("missing", exception.Message);
    }

    [Fact]
    public async Task ConnectAsyncRejectsProfileWithoutRegion()
    {
        await File.WriteAllTextAsync(
            _profilesPath,
            """
            [without-region]
            aws_access_key_id = AKIAEXAMPLE
            aws_secret_access_key = example-secret
            """);
        var service = new AwsConnectionService(_profilesPath);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ConnectAsync("without-region", _ => { }));

        Assert.Contains("region", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        File.Delete(_profilesPath);
    }
}
