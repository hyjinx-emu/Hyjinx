using Microsoft.Extensions.Configuration;

namespace Hyjinx.UI.Common.Configuration.UI;

public record ColumnSort
{
    [ConfigurationKeyName("sort_column_id")]
    public int SortColumnId { get; set; }
    
    [ConfigurationKeyName("sort_ascending")]
    public bool SortAscending { get; set; }
}
