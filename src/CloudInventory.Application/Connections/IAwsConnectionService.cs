namespace CloudInventory.Application.Connections;

public interface IAwsConnectionService
{
    Task<AwsConnectionIdentity> ConnectAsync(
        string profileName,
        Action<AwsSignInInstruction> signInRequired,
        CancellationToken cancellationToken = default);
}
