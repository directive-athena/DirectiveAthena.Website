// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthenaWeb.Services.Content;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[Flags]
public enum QueryConfig {
    None                = 0b0,
                        
    Reversed            = 0b1,
    WithSoftDeleted     = 0b10,
    WithHidden          = 0b100,
    SortByCreatedAt     = 0b1000,
    SortByModifiedAt    = 0b10000
}

public static class QueryConfigExtensions {
    public static bool HasFlagFast(this QueryConfig value, QueryConfig flag) {
        return (value & flag) != 0;
    }
}
