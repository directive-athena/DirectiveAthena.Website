// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthenaWeb.Services.Content;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public interface IContentRepository<T> where T : ContentBase {
    ValueTask<T[]> GetAllAsync(CancellationToken ct = default);
    ValueTask<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    ValueTask<bool> DeleteByIdAsync(Guid id, CancellationToken ct = default);
    
    ValueTask<bool> SaveAsync(IEnumerable<T> items, CancellationToken ct = default);
    ValueTask<string> GetAsJsonStringAsync(CancellationToken ct = default);
}
