namespace CloudInventory.Application.Connections;

public enum AwsAuthenticationKind
{
    Unknown,
    Sso,
    AssumeRole,
    CredentialProcess,
    WebIdentity,
    StaticCredentials,
}
