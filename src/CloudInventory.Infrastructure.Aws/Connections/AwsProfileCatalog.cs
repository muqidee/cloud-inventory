using Amazon.Runtime.CredentialManagement;
using CloudInventory.Application.Connections;

namespace CloudInventory.Infrastructure.Aws.Connections;

public sealed class AwsProfileCatalog : IAwsProfileCatalog
{
    private readonly CredentialProfileStoreChain _profileStore;

    public AwsProfileCatalog(string? profilesLocation = null)
    {
        _profileStore = string.IsNullOrWhiteSpace(profilesLocation)
            ? new CredentialProfileStoreChain()
            : new CredentialProfileStoreChain(profilesLocation);
    }

    public Task<IReadOnlyList<AwsProfileSummary>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<AwsProfileSummary> profiles = _profileStore
            .ListProfiles()
            .Select(Map)
            .OrderBy(profile => profile.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return Task.FromResult(profiles);
    }

    private static AwsProfileSummary Map(CredentialProfile profile)
    {
        var options = profile.Options;

        return new AwsProfileSummary(
            profile.Name,
            GetAuthenticationKind(options),
            profile.Region?.SystemName,
            options.SsoAccountId,
            options.SsoRoleName,
            options.SsoStartUrl);
    }

    private static AwsAuthenticationKind GetAuthenticationKind(
        CredentialProfileOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.SsoSession) ||
            !string.IsNullOrWhiteSpace(options.SsoStartUrl) ||
            !string.IsNullOrWhiteSpace(options.SsoAccountId))
        {
            return AwsAuthenticationKind.Sso;
        }

        if (!string.IsNullOrWhiteSpace(options.RoleArn))
        {
            return AwsAuthenticationKind.AssumeRole;
        }

        if (!string.IsNullOrWhiteSpace(options.CredentialProcess))
        {
            return AwsAuthenticationKind.CredentialProcess;
        }

        if (!string.IsNullOrWhiteSpace(options.WebIdentityTokenFile))
        {
            return AwsAuthenticationKind.WebIdentity;
        }

        if (!string.IsNullOrWhiteSpace(options.AccessKey))
        {
            return AwsAuthenticationKind.StaticCredentials;
        }

        return AwsAuthenticationKind.Unknown;
    }
}
