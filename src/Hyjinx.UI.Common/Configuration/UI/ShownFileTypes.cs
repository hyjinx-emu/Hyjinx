using Microsoft.Extensions.Configuration;

namespace Hyjinx.UI.Common.Configuration.UI;

public record ShownFileTypes
{
    public bool NSP { get; set; }

    public bool PFS0 { get; set; }

    public bool XCI { get; set; }

    public bool NCA { get; set; }

    public bool NRO { get; set; }

    public bool NSO { get; set; }
}