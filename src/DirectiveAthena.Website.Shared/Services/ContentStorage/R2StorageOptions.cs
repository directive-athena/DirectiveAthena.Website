// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthena.Website.Services.ContentStorage;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public sealed record R2StorageOptions(
    string? AccountId,
    string? AccessKeyId,
    string? SecretAccessKey,
    string? BucketName,
    string? PublicBaseUrl,
    bool EnableWrites,
    string Region = "auto"
) {
    public bool IsReadConfigured => !PublicBaseUrl.IsNullOrWhiteSpace();

    public bool IsWriteConfigured =>
        EnableWrites
        && !AccountId.IsNullOrWhiteSpace()
        && !AccessKeyId.IsNullOrWhiteSpace()
        && !SecretAccessKey.IsNullOrWhiteSpace()
        && !BucketName.IsNullOrWhiteSpace();
}
