namespace CloudInventory.Application.Connections;

public interface IAwsProfileCatalog
{
    Task<IReadOnlyList<AwsProfileSummary>> ListAsync(
        CancellationToken cancellationToken = default);
}
