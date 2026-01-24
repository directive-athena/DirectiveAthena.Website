// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using DirectiveAthena.Website.Services.FileSystem;
using DirectiveAthenaTests.Website.Helpers;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using NSubstitute;

namespace DirectiveAthenaTests.Website.Services;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class LocalFileStorageTests {
    [Test]
    public async Task IsSupportedAsync_InvokesJsInterop() {
        // Arrange
        var jsRuntime = Substitute.For<IJSRuntime>();
        jsRuntime.InvokeAsync<bool>("fsApi.isSupported", Arg.Any<CancellationToken>()).Returns(new ValueTask<bool>(true));

        var manager = new LocalFileStorage(jsRuntime, new FakeNavigationManager());

        // Act
        bool result = await manager.IsSupportedAsync();

        // Assert
        await Assert.That(result).IsTrue();
        await jsRuntime.Received(1).InvokeAsync<bool>("fsApi.isSupported", Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task RequestAccessAsync_InvokesJsInterop() {
        // Arrange
        var jsRuntime = Substitute.For<IJSRuntime>();
        jsRuntime.InvokeAsync<bool>("fsApi.requestAccess", Arg.Any<CancellationToken>()).Returns(new ValueTask<bool>(true));

        var manager = new LocalFileStorage(jsRuntime, new FakeNavigationManager());

        // Act
        bool result = await manager.RequestAccessAsync();

        // Assert
        await Assert.That(result).IsTrue();
        await jsRuntime.Received(1).InvokeAsync<bool>("fsApi.requestAccess", Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task HasAccessAsync_InvokesJsInterop() {
        // Arrange
        var jsRuntime = Substitute.For<IJSRuntime>();
        jsRuntime.InvokeAsync<bool>("fsApi.hasAccess", Arg.Any<CancellationToken>()).Returns(new ValueTask<bool>(true));

        var manager = new LocalFileStorage(jsRuntime, new FakeNavigationManager());
        
        // Act
        bool result = await manager.HasAccessAsync();

        // Assert
        await Assert.That(result).IsTrue();
        await jsRuntime.Received(1).InvokeAsync<bool>("fsApi.hasAccess", Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task VerifyPermissionAsync_InvokesJsInterop() {
        // Arrange
        var jsRuntime = Substitute.For<IJSRuntime>();
        jsRuntime.InvokeAsync<bool>("fsApi.verifyPermission", Arg.Any<CancellationToken>()).Returns(new ValueTask<bool>(true));

        var manager = new LocalFileStorage(jsRuntime, new FakeNavigationManager());

        // Act
        bool result = await manager.VerifyPermissionAsync();

        // Assert
        await Assert.That(result).IsTrue();
        await jsRuntime.Received(1).InvokeAsync<bool>("fsApi.verifyPermission", Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task ResetAccessAsync_InvokesJsInterop() {
        // Arrange
        var jsRuntime = Substitute.For<IJSRuntime>();
        jsRuntime.InvokeAsync<IJSVoidResult>("fsApi.resetAccess", Arg.Any<CancellationToken>())
            .Returns(new ValueTask<IJSVoidResult>());

        var manager = new LocalFileStorage(jsRuntime, new FakeNavigationManager());
        
        // Act
        await manager.ResetAccessAsync();

        // Assert
        await jsRuntime.Received(1).InvokeAsync<IJSVoidResult>("fsApi.resetAccess", Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task WriteFileAsync_InvokesJsInterop() {
        // Arrange
        var jsRuntime = Substitute.For<IJSRuntime>();
        jsRuntime.InvokeAsync<bool>("fsApi.writeFile", Arg.Any<CancellationToken>(), Arg.Any<object?[]>())
            .Returns(new ValueTask<bool>(true));

        var manager = new LocalFileStorage(jsRuntime, new FakeNavigationManager());

        // Act
        bool result = await manager.WriteFileAsync("path.md", "content");

        // Assert
        await Assert.That(result).IsTrue();
        await jsRuntime.Received(1).InvokeAsync<bool>(
            "fsApi.writeFile",
            Arg.Any<CancellationToken>(),
            Arg.Is<object?[]>(args => (string)args[0]! == "path.md" && (string)args[1]! == "content"));
    }

    [Test]
    public async Task ReadFileAsync_InvokesJsInterop() {
        // Arrange
        var jsRuntime = Substitute.For<IJSRuntime>();
        jsRuntime.InvokeAsync<string?>("fsApi.readFile", Arg.Any<CancellationToken>(), Arg.Any<object?[]>())
            .Returns(new ValueTask<string?>("content"));

        var manager = new LocalFileStorage(jsRuntime, new FakeNavigationManager());

        // Act
        string? result = await manager.ReadFileAsync("path.md");

        // Assert
        await Assert.That(result).IsEqualTo("content");
        await jsRuntime.Received(1).InvokeAsync<string?>(
            "fsApi.readFile",
            Arg.Any<CancellationToken>(),
            Arg.Is<object?[]>(args => (string)args[0]! == "path.md"));
    }

    [Test]
    public async Task DeleteFileAsync_InvokesJsInterop() {
        // Arrange
        var jsRuntime = Substitute.For<IJSRuntime>();
        jsRuntime.InvokeAsync<bool>("fsApi.deleteFile", Arg.Any<CancellationToken>(), Arg.Any<object?[]>())
            .Returns(new ValueTask<bool>(true));

        var manager = new LocalFileStorage(jsRuntime, new FakeNavigationManager());

        // Act
        bool result = await manager.DeleteFileAsync("path.md");

        // Assert
        await Assert.That(result).IsTrue();
        await jsRuntime.Received(1).InvokeAsync<bool>(
            "fsApi.deleteFile",
            Arg.Any<CancellationToken>(),
            Arg.Is<object?[]>(args => (string)args[0]! == "path.md"));
    }
}
