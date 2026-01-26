// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthena.Website.Services.FileSystem;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public sealed class R2StorageOptions {
    public string? AccountId { get; init; }
    public string? AccessKeyId { get; init; }
    public string? SecretAccessKey { get; init; }
    public string? BucketName { get; init; }
    public string? PublicBaseUrl { get; init; }
    public bool EnableWrites { get; init; }
    public string Region { get; init; } = "auto";

    public bool IsReadConfigured => !string.IsNullOrWhiteSpace(PublicBaseUrl);

    public bool IsWriteConfigured =>
        EnableWrites
        && !string.IsNullOrWhiteSpace(AccountId)
        && !string.IsNullOrWhiteSpace(AccessKeyId)
        && !string.IsNullOrWhiteSpace(SecretAccessKey)
        && !string.IsNullOrWhiteSpace(BucketName);
}
