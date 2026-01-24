// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using Microsoft.AspNetCore.Components;

namespace DirectiveAthenaTests.Website.Helpers;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public  class FakeNavigationManager : NavigationManager {
    public bool LastForceLoad { get; private set; }

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public FakeNavigationManager(string baseUri = "http://localhost/", string uri = "http://localhost/") {
        Initialize(baseUri, uri);
    }

    protected override void NavigateToCore(string uri, bool forceLoad) {
        LastForceLoad = forceLoad;
        Uri = ToAbsoluteUri(uri).ToString();
    }
}
