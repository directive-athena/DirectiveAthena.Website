// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Text.Json;
using DirectiveAthenaWeb;

namespace DirectiveAthenaWebTests.Shared;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class LocalizedDataHolderTests {
    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    [Test]
    public async Task Serialize_ProducesLocaleProperties() {
        // Arrange
        var holder = new LocalizedDataHolder(new Dictionary<string, string> {
            ["en"] = "Hello",
            ["de"] = "Hallo"
        });

        // Act
        string json = JsonSerializer.Serialize(holder);
        using JsonDocument document = JsonDocument.Parse(json);

        // Assert
        await Assert.That(document.RootElement.ValueKind).IsEqualTo(JsonValueKind.Object);
        await Assert.That(document.RootElement.GetProperty("en").GetString()).IsEqualTo("Hello");
        await Assert.That(document.RootElement.GetProperty("de").GetString()).IsEqualTo("Hallo");
        await Assert.That(document.RootElement.EnumerateObject().Count()).IsEqualTo(2);
    }

    [Test]
    public async Task Deserialize_ReadsLocaleProperties_CaseInsensitive() {
        // Arrange
        const string json = "{\"EN\":\"Hello\",\"de\":\"Hallo\"}";

        // Act
        LocalizedDataHolder? holder = JsonSerializer.Deserialize<LocalizedDataHolder>(json);

        // Assert
        await Assert.That(holder).IsNotNull();
        await Assert.That(holder!.GetWithFallback("en")).IsEqualTo("Hello");
        await Assert.That(holder.GetWithFallback("DE")).IsEqualTo("Hallo");
        await Assert.That(holder.Values.Count).IsEqualTo(2);
    }

    [Test]
    public async Task RoundTrip_PreservesValues() {
        // Arrange
        var holder = new LocalizedDataHolder();
        holder.Set("en", "Hello");
        holder.Set("nl", "Hallo");

        // Act
        string json = JsonSerializer.Serialize(holder);
        var roundTrip = JsonSerializer.Deserialize<LocalizedDataHolder>(json);

        // Assert
        await Assert.That(roundTrip).IsNotNull();
        await Assert.That(roundTrip!.GetWithFallback("en")).IsEqualTo("Hello");
        await Assert.That(roundTrip.GetWithFallback("nl")).IsEqualTo("Hallo");
        await Assert.That(roundTrip.Values.Count).IsEqualTo(2);
    }
}
