// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using Microsoft.JSInterop;

namespace DirectiveAthenaWebTests.Helpers;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public  class RecordingJsRuntime : IJSRuntime {
    public List<Invocation> Invocations { get; } = [];

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) {
        Invocations.Add(new Invocation(identifier, args));
        return new ValueTask<TValue>(default(TValue)!);
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) {
        Invocations.Add(new Invocation(identifier, args));
        return new ValueTask<TValue>(default(TValue)!);
    }

    public record Invocation(string Identifier, object?[]? Args);
}
