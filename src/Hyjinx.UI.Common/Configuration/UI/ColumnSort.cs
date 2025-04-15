using Microsoft.Extensions.Configuration;

namespace Hyjinx.UI.Common.Configuration.UI;

public record ColumnSort
{
    public int SortColumnId { get; set; }
    
    public bool SortAscending { get; set; }
}
