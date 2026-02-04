// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using DirectiveAthenaWeb.Services.ContentStorage;

namespace DirectiveAthenaWeb.Services.Content;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableSingleton<IR2StatusTracker>]
public class R2StatusTracker : IR2StatusTracker {
    private int _isFallbackActive;
    public bool IsFallbackActive => Volatile.Read(ref _isFallbackActive) == 1;
    
    public string? LastError { get; private set; }
    public event Action? StatusChanged;

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public void ActivateFallback(string? message) {
        if (Interlocked.Exchange(ref _isFallbackActive, 1) == 1) return;

        LastError = message;
        StatusChanged?.Invoke();
    }
}
