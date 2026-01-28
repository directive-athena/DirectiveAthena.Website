// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthenaWeb.Services.Contact;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class ContactInfoOptions {
    public List<ContactInfoItem> Items { get; set; } = [];

    public ContactInfoOptions AddContactInfo(
        string title,
        string url,
        string simpleIconsUrl,
        bool includeInFooter = ContactInfoItem.IncludeInFooterDefault
    ) {
        Items.Add(new ContactInfoItem { Title = title, Url = url, SimpleIconsUrl = simpleIconsUrl, IncludeInFooter = includeInFooter });
        return this;
    }
}
