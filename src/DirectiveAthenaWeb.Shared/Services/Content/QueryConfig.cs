// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace DirectiveAthenaWeb.Services.Content;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[Flags]
public enum QueryConfig {
    None = 0b0,

    Reversed = 0b1,
    WithSoftDeleted = 0b10,
    WithHidden = 0b100,
    WithDevContent = 0b1000,

    SortByCreatedAt = 0b10000,
    SortByModifiedAt = 0b100000,
    SortByInternalTitle = 0b1000000
}

public static class QueryConfigExtensions {
    extension(QueryConfig value) {
        public bool HasFlagFast(QueryConfig flag) 
            => (value & flag) != 0;

        public QueryConfig AddEnvironmentFlags() {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Development") return value;
            return value | QueryConfig.WithDevContent;
        }
    }

}
