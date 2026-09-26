namespace CloudInventory.Application.Connections;

public sealed record AwsConnectionIdentity(
    string ProfileName,
    string AccountId,
    string Arn,
    string UserId,
    string Region);
