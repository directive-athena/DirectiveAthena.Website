// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Immutable;

namespace DirectiveAthenaWeb.Services.Contact;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[InjectableSingleton<IContactInfoProvider>]
public class ContactInfoProvider(ILogger<ContactInfoProvider> logger, IOptions<ContactInfoOptions> optionsAccessor) : IContactInfoProvider {
    private readonly ImmutableArray<ContactInfoItem> _contactInfos = BuildContactInfos(optionsAccessor.Value);

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public IEnumerable<ContactInfoItem> GetContactInfos() {
        logger.Debug("Providing {Count} contact info entries.", _contactInfos.Length);
        return _contactInfos.AsEnumerable();
    }

    private static ImmutableArray<ContactInfoItem> BuildContactInfos(ContactInfoOptions options) {
        if (options.Items.Count == 0) return ImmutableArray<ContactInfoItem>.Empty;
        return [..options.Items];
    }
}
