// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using JetBrains.Annotations;

namespace DirectiveAthenaWeb.Services.R2Storage;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class R2StorageOptions {
    [UsedImplicitly] public string? AccountId { get; init; }
    [UsedImplicitly] public string? AccessKeyId { get; init; }
    [UsedImplicitly] public string? SecretAccessKey { get; init; }
    [UsedImplicitly] public string? BucketName { get; init; }
    [UsedImplicitly] public string? PublicBaseUrl { get; init; }
    [UsedImplicitly] public bool EnableWrites { get; init; }
    [UsedImplicitly] public string Region { get; init; } = "auto";
    [UsedImplicitly] public string? ProxyEndpoint { get; init; }

    public bool IsWriteConfigured =>
        EnableWrites
        && !AccountId.IsNullOrWhiteSpace()
        && !AccessKeyId.IsNullOrWhiteSpace()
        && !SecretAccessKey.IsNullOrWhiteSpace()
        && !BucketName.IsNullOrWhiteSpace();

    public bool CanWrite => IsWriteConfigured || (!ProxyEndpoint.IsNullOrWhiteSpace() && EnableWrites);
}
