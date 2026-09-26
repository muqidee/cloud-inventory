namespace CloudInventory.Application.Connections;

public sealed record AwsSignInInstruction(
    Uri VerificationUri,
    string? UserCode);
