using CloudInventory.Application.Connections;
using CloudInventory.Infrastructure.Aws.Connections;

namespace CloudInventory.Infrastructure.Aws.Tests.Connections;

public sealed class AwsProfileCatalogTests : IDisposable
{
    private readonly string _profilesPath = Path.GetTempFileName();

    [Fact]
    public async Task ListAsyncMapsAndSortsProfiles()
    {
        await File.WriteAllTextAsync(
            _profilesPath,
            """
            [static-dev]
            aws_access_key_id = AKIAEXAMPLE
            aws_secret_access_key = example-secret
            region = eu-west-1

            [sso-production]
            sso_start_url = https://example.awsapps.com/start
            sso_region = eu-west-1
            sso_account_id = 123456789012
            sso_role_name = ReadOnly
            region = eu-west-1
            """);

        var catalog = new AwsProfileCatalog(_profilesPath);

        var profiles = await catalog.ListAsync();

        Assert.Collection(
            profiles,
            profile =>
            {
                Assert.Equal("sso-production", profile.Name);
                Assert.Equal(AwsAuthenticationKind.Sso, profile.AuthenticationKind);
                Assert.Equal("eu-west-1", profile.Region);
                Assert.Equal("123456789012", profile.AccountId);
                Assert.Equal("ReadOnly", profile.RoleName);
                Assert.Equal("https://example.awsapps.com/start", profile.SsoStartUrl);
            },
            profile =>
            {
                Assert.Equal("static-dev", profile.Name);
                Assert.Equal(AwsAuthenticationKind.StaticCredentials, profile.AuthenticationKind);
                Assert.Equal("eu-west-1", profile.Region);
            });
    }

    public void Dispose()
    {
        File.Delete(_profilesPath);
    }
}
