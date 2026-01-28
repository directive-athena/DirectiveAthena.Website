// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace DirectiveAthenaWeb.Services.Contact;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableScoped<IContactInfoProvider>]
public class ContactInfoProvider(ILogger<ContactInfoProvider> logger, IOptions<ContactInfoOptions> optionsAccessor, HttpClient httpClient) : IContactInfoProvider {
    private readonly ImmutableArray<ContactInfoItem> _contactInfos = BuildContactInfos(optionsAccessor.Value);
    
    private ConcurrentDictionary<string, string> SvgCache { get; } = new();
    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public IEnumerable<ContactInfoItem> GetContactInfos() {
        logger.Debug("Providing {Count} contact info entries.", _contactInfos.Length);
        return _contactInfos.AsEnumerable();
    }

    private static ImmutableArray<ContactInfoItem> BuildContactInfos(ContactInfoOptions options)
        => !options.Items.IsEmpty()
            ? [..options.Items]
            : ImmutableArray<ContactInfoItem>.Empty;

    public async ValueTask<string> GetSvgData(ContactInfoItem item, CancellationToken ct = default) {
        if (SvgCache.TryGetValue(item.SimpleIconsUrl, out string? svg)) return svg;
        try {
            using HttpResponseMessage response = await httpClient.GetAsync(item.SimpleIconsUrl, ct);
            response.EnsureSuccessStatusCode();
            svg = await response.Content.ReadAsStringAsync(ct);
            SvgCache.AddOrUpdate(item.SimpleIconsUrl, svg, (_, _) => svg);
            return svg;
        }
        catch (Exception ex) {
            logger.Warning(ex, "Failed to fetch SVG data for {Url}", item.SimpleIconsUrl);
            return string.Empty;
        }
    } 
}
