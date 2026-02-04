// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthenaWeb.Services.Content;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public interface IContentRepository<T> where T : IContent {
    ValueTask<T[]> GetAllAsync(QueryConfig config = default, CancellationToken ct = default);
    ValueTask<T?> GetByIdAsync(Guid id, QueryConfig config = default, CancellationToken ct = default);
    ValueTask<bool> SoftDeleteByIdAsync(Guid id, CancellationToken ct = default);
    ValueTask<bool> HardDeleteByIdAsync(Guid id, CancellationToken ct = default);
    ValueTask<bool> RestoreByIdAsync(Guid id, CancellationToken ct = default);

    ValueTask<bool> AddOrUpdateAsync(T item, CancellationToken ct = default);
    ValueTask<bool> AddOrUpdateRangeAsync(IEnumerable<T> items, CancellationToken ct = default);
    ValueTask<bool> SaveAsync(CancellationToken ct = default);
    
    ValueTask<string> GetAsJsonStringAsync(CancellationToken ct = default);
}
