namespace CloudInventory.Application.Connections;

public sealed record AwsProfileSummary(
    string Name,
    AwsAuthenticationKind AuthenticationKind,
    string? Region,
    string? AccountId,
    string? RoleName,
    string? SsoStartUrl);
