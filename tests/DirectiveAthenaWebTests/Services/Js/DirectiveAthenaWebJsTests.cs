// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthenaWeb.Services.Js;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using NSubstitute;

namespace DirectiveAthenaWebTests.Services.Js;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class DirectiveAthenaWebJsTests {
    private sealed class TestScrollListener;
    private sealed class FakeJsRuntime : IJSRuntime {
        public List<JsCall> Calls { get; } = [];
        public Func<JsCall, Exception?>? OnInvoke { get; set; }
        public static Func<JsCall, object?>? OnReturn => null;

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) {
            var call = new JsCall(identifier, args);
            Calls.Add(call);

            Exception? exception = OnInvoke?.Invoke(call);
            if (exception is not null) {
                throw exception;
            }

            object? result = OnReturn?.Invoke(call);
            if (result is TValue typed) {
                return new ValueTask<TValue>(typed);
            }

            return new ValueTask<TValue>(default(TValue)!);
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
            => InvokeAsync<TValue>(identifier, CancellationToken.None, args);
    }

    private sealed record JsCall(string Identifier, object?[]? Args);

    // -----------------------------------------------------------------------------------------------------------------
    // Test Methods
    // -----------------------------------------------------------------------------------------------------------------
    [Test]
    [Arguments("CopyToClipboard", "copyToClipboard", "text", null)]
    [Arguments("IsFileSystemSupported", "fsApi.isSupported", null, null)]
    [Arguments("RequestFileSystemAccess", "fsApi.requestAccess", null, null)]
    [Arguments("HasFileSystemAccess", "fsApi.hasAccess", null, null)]
    [Arguments("VerifyFileSystemPermission", "fsApi.verifyPermission", null, null)]
    [Arguments("ResetFileSystemAccess", "fsApi.resetAccess", null, null)]
    [Arguments("WriteFile", "fsApi.writeFile", "path", "content")]
    [Arguments("DeleteFile", "fsApi.deleteFile", "path", null)]
    [Arguments("Confirm", "confirm", "message", null)]
    public async Task BoolMethods_InvokeExpectedJsAndReturnValue(string methodName, string identifier, string? arg1, string? arg2) {
        // Arrange
        DirectiveAthenaWebJs service = CreateService(out IJSRuntime jsRuntime);
        const bool expected = true;
        SetupBoolInvoke(jsRuntime, identifier, expected, arg1, arg2);

        // Act
        bool result = await InvokeBool(service, methodName, arg1, arg2);

        // Assert
        await Assert.That(result).IsEqualTo(expected);
        await VerifyBoolInvoke(jsRuntime, identifier, arg1, arg2);
    }

    [Test]
    [Arguments("ReadFile", "fsApi.readFile", "path")]
    [Arguments("GetLocalStorageItem", "localStorage.getItem", "key")]
    public async Task StringMethods_InvokeExpectedJsAndReturnValue(string methodName, string identifier, string arg1) {
        // Arrange
        DirectiveAthenaWebJs service = CreateService(out IJSRuntime jsRuntime);
        const string expected = "value";
        jsRuntime.InvokeAsync<string?>(identifier, Arg.Any<CancellationToken>(), Arg.Is<object?[]?>(args => ArgsMatch(args, arg1)))
            .Returns(new ValueTask<string?>(expected));

        // Act
        string? result = await InvokeString(service, methodName, arg1);

        // Assert
        await Assert.That(result).IsEqualTo(expected);
        await jsRuntime.Received(1).InvokeAsync<string?>(identifier, Arg.Any<CancellationToken>(), Arg.Is<object?[]?>(args => ArgsMatch(args, arg1)));
    }

    [Test]
    [Arguments("DownloadFile", "downloadFile", "file.txt", "content")]
    [Arguments("ScrollToElement", "scrollToElement", "elementId", null)]
    [Arguments("SetLocalStorageItem", "localStorage.setItem", "key", "value")]
    public async Task VoidMethods_InvokeExpectedJs(string methodName, string identifier, string? arg1, string? arg2) {
        // Arrange
        var jsRuntime = new FakeJsRuntime();
        DirectiveAthenaWebJs service = CreateService(jsRuntime);

        // Act
        await InvokeVoid(service, methodName, arg1, arg2);

        // Assert
        await AssertJsCall(jsRuntime, identifier, BuildArgs(arg1, arg2));
    }

    [Test]
    public async Task RegisterScrollListener_InvokesJs() {
        // Arrange
        var jsRuntime = new FakeJsRuntime();
        DirectiveAthenaWebJs service = CreateService(jsRuntime);
        using var dotNetHelper = DotNetObjectReference.Create(new TestScrollListener());

        // Act
        await service.RegisterScrollListenerAsync(dotNetHelper);

        // Assert
        await AssertJsCall(jsRuntime, "registerScrollListener", [dotNetHelper]);
    }

    [Test]
    public async Task CopyToClipboard_WhenCanceled_ReturnsFalse() {
        // Arrange
        DirectiveAthenaWebJs service = CreateService(out IJSRuntime jsRuntime);
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        jsRuntime.InvokeAsync<bool>("copyToClipboard", Arg.Any<CancellationToken>(), Arg.Is<object?[]?>(args => ArgsMatch(args, "text")))
            .Returns<ValueTask<bool>>(_ => throw new OperationCanceledException(cts.Token));

        // Act
        bool result = await service.CopyToClipboardAsync("text", cts.Token);

        // Assert
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task ReadFile_WhenCanceled_ReturnsNull() {
        // Arrange
        DirectiveAthenaWebJs service = CreateService(out IJSRuntime jsRuntime);
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        jsRuntime.InvokeAsync<string?>("fsApi.readFile", Arg.Any<CancellationToken>(), Arg.Is<object?[]?>(args => ArgsMatch(args, "path")))
            .Returns<ValueTask<string?>>(_ => throw new OperationCanceledException(cts.Token));

        // Act
        string? result = await service.ReadFileAsync("path", cts.Token);

        // Assert
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task DownloadFile_WhenCanceled_DoesNotThrow() {
        // Arrange
        var jsRuntime = new FakeJsRuntime();
        DirectiveAthenaWebJs service = CreateService(jsRuntime);
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        jsRuntime.OnInvoke = call => call.Identifier == "downloadFile" ? new OperationCanceledException(cts.Token) : null;

        // Act
        await service.DownloadFileAsync("file.txt", "content", cts.Token);

        // Assert
        await AssertJsCall(jsRuntime, "downloadFile", ["file.txt", "content"]);
    }

    [Test]
    public async Task CopyToClipboard_WhenException_ReturnsFalse() {
        // Arrange
        DirectiveAthenaWebJs service = CreateService(out IJSRuntime jsRuntime);
        jsRuntime.InvokeAsync<bool>("copyToClipboard", Arg.Any<CancellationToken>(), Arg.Is<object?[]?>(args => ArgsMatch(args, "text")))
            .Returns<ValueTask<bool>>(_ => throw new InvalidOperationException("boom"));

        // Act
        bool result = await service.CopyToClipboardAsync("text");

        // Assert
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task ReadFile_WhenException_ReturnsNull() {
        // Arrange
        DirectiveAthenaWebJs service = CreateService(out IJSRuntime jsRuntime);
        jsRuntime.InvokeAsync<string?>("fsApi.readFile", Arg.Any<CancellationToken>(), Arg.Is<object?[]?>(args => ArgsMatch(args, "path")))
            .Returns<ValueTask<string?>>(_ => throw new InvalidOperationException("boom"));

        // Act
        string? result = await service.ReadFileAsync("path");

        // Assert
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task DownloadFile_WhenException_DoesNotThrow() {
        // Arrange
        var jsRuntime = new FakeJsRuntime();
        DirectiveAthenaWebJs service = CreateService(jsRuntime);
        jsRuntime.OnInvoke = call => call.Identifier == "downloadFile" ? new InvalidOperationException("boom") : null;

        // Act
        await service.DownloadFileAsync("file.txt", "content");

        // Assert
        await AssertJsCall(jsRuntime, "downloadFile", ["file.txt", "content"]);
    }

    // -----------------------------------------------------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------------------------------------------------
    private static DirectiveAthenaWebJs CreateService(out IJSRuntime jsRuntime) {
        jsRuntime = Substitute.For<IJSRuntime>();
        var logger = Substitute.For<ILogger<DirectiveAthenaWebJs>>();
        return new DirectiveAthenaWebJs(jsRuntime, logger);
    }

    private static DirectiveAthenaWebJs CreateService(IJSRuntime jsRuntime) {
        ILogger<DirectiveAthenaWebJs> logger = Substitute.For<ILogger<DirectiveAthenaWebJs>>();
        return new DirectiveAthenaWebJs(jsRuntime, logger);
    }

    private static ValueTask<bool> InvokeBool(DirectiveAthenaWebJs service, string methodName, string? arg1, string? arg2)
        => methodName switch {
            "CopyToClipboard" => service.CopyToClipboardAsync(arg1 ?? string.Empty),
            "IsFileSystemSupported" => service.IsFileSystemSupportedAsync(),
            "RequestFileSystemAccess" => service.RequestFileSystemAccessAsync(),
            "HasFileSystemAccess" => service.HasFileSystemAccessAsync(),
            "VerifyFileSystemPermission" => service.VerifyFileSystemPermissionAsync(),
            "ResetFileSystemAccess" => service.ResetFileSystemAccessAsync(),
            "WriteFile" => service.WriteFileAsync(arg1 ?? string.Empty, arg2 ?? string.Empty),
            "DeleteFile" => service.DeleteFileAsync(arg1 ?? string.Empty),
            "Confirm" => service.ConfirmAsync(arg1 ?? string.Empty),
            _ => throw new ArgumentOutOfRangeException(nameof(methodName), methodName, @"Unknown bool method.")
        };

    private static ValueTask<string?> InvokeString(DirectiveAthenaWebJs service, string methodName, string arg1)
        => methodName switch {
            "ReadFile" => service.ReadFileAsync(arg1),
            "GetLocalStorageItem" => service.GetLocalStorageItemAsync(arg1),
            _ => throw new ArgumentOutOfRangeException(nameof(methodName), methodName, @"Unknown string method.")
        };

    private static ValueTask InvokeVoid(DirectiveAthenaWebJs service, string methodName, string? arg1, string? arg2)
        => methodName switch {
            "DownloadFile" => service.DownloadFileAsync(arg1 ?? string.Empty, arg2 ?? string.Empty),
            "ScrollToElement" => service.ScrollToElementAsync(arg1 ?? string.Empty),
            "SetLocalStorageItem" => service.SetLocalStorageItemAsync(arg1 ?? string.Empty, arg2 ?? string.Empty),
            _ => throw new ArgumentOutOfRangeException(nameof(methodName), methodName, @"Unknown void method.")
        };

    private static void SetupBoolInvoke(IJSRuntime jsRuntime, string identifier, bool expected, string? arg1, string? arg2) {
        object?[] expectedArgs = BuildArgs(arg1, arg2);
        jsRuntime.InvokeAsync<bool>(identifier, Arg.Any<CancellationToken>(), Arg.Is<object?[]?>(args => ArgsMatch(args, expectedArgs)))
            .Returns(new ValueTask<bool>(expected));
    }

    private static async ValueTask VerifyBoolInvoke(IJSRuntime jsRuntime, string identifier, string? arg1, string? arg2) {
        object?[] expectedArgs = BuildArgs(arg1, arg2);
        await jsRuntime.Received(1).InvokeAsync<bool>(identifier, Arg.Any<CancellationToken>(), Arg.Is<object?[]?>(args => ArgsMatch(args, expectedArgs)));
    }

    private static object?[] BuildArgs(string? arg1, string? arg2) {
        if (arg1 is null) {
            return Array.Empty<object?>();
        }

        if (arg2 is null) {
            return [arg1];
        }

        return [arg1, arg2];
    }

    private static async ValueTask AssertJsCall(FakeJsRuntime jsRuntime, string identifier, object?[] expectedArgs) {
        await Assert.That(jsRuntime.Calls.Count).IsEqualTo(1);

        JsCall call = jsRuntime.Calls[0];
        await Assert.That(call.Identifier).IsEqualTo(identifier);
        await Assert.That(ArgsMatch(call.Args, expectedArgs)).IsTrue();
    }

    private static bool ArgsMatch(object?[]? actual, params object?[] expected) {
        if (expected.Length == 0) {
            return actual is null || actual.Length == 0;
        }

        if (actual is null || actual.Length != expected.Length) {
            return false;
        }

        for (int index = 0; index < expected.Length; index++) {
            if (!Equals(actual[index], expected[index])) {
                return false;
            }
        }

        return true;
    }
}
