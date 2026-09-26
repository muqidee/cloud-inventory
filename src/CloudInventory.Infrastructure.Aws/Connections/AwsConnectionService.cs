using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using CloudInventory.Application.Connections;

namespace CloudInventory.Infrastructure.Aws.Connections;

public sealed class AwsConnectionService : IAwsConnectionService
{
    private readonly CredentialProfileStoreChain _profileStore;

    public AwsConnectionService(string? profilesLocation = null)
    {
        _profileStore = string.IsNullOrWhiteSpace(profilesLocation)
            ? new CredentialProfileStoreChain()
            : new CredentialProfileStoreChain(profilesLocation);
    }

    public async Task<AwsConnectionIdentity> ConnectAsync(
        string profileName,
        Action<AwsSignInInstruction> signInRequired,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profileName);
        ArgumentNullException.ThrowIfNull(signInRequired);

        if (!_profileStore.TryGetProfile(profileName, out var profile))
        {
            throw new InvalidOperationException($"AWS profile '{profileName}' was not found.");
        }

        if (profile.Region is null)
        {
            throw new InvalidOperationException($"AWS profile '{profileName}' does not define a region.");
        }

        if (!_profileStore.TryGetAWSCredentials(profileName, out var credentials))
        {
            throw new InvalidOperationException($"AWS credentials for profile '{profileName}' could not be resolved.");
        }

        ConfigureSsoCredentials(credentials, signInRequired);

        using var sts = new AmazonSecurityTokenServiceClient(credentials, profile.Region);
        var response = await sts.GetCallerIdentityAsync(
            new GetCallerIdentityRequest(),
            cancellationToken);

        return new AwsConnectionIdentity(
            profileName,
            response.Account,
            response.Arn,
            response.UserId,
            profile.Region.SystemName);
    }

    private static void ConfigureSsoCredentials(
        AWSCredentials credentials,
        Action<AwsSignInInstruction> signInRequired)
    {
        if (credentials is not SSOAWSCredentials ssoCredentials)
        {
            return;
        }

        var options = ssoCredentials.Options;

        options.ClientName = "Cloud Inventory";
        options.SupportsGettingNewToken = true;
        options.SsoVerificationCallback = arguments =>
        {
            var address = string.IsNullOrWhiteSpace(arguments.VerificationUriComplete)
                ? arguments.VerificationUri
                : arguments.VerificationUriComplete;

            signInRequired(new AwsSignInInstruction(
                new Uri(address, UriKind.Absolute),
                arguments.UserCode));
        };
    }
}
