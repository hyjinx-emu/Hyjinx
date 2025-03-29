using Microsoft.Extensions.Configuration;

namespace Hyjinx.UI.Common.Configuration.UI;

public record ShownFileTypes
{
    [ConfigurationKeyName("nsp")]
    public bool NSP { get; set; }
    
    [ConfigurationKeyName("pfs0")]
    public bool PFS0 { get; set; }
    
    [ConfigurationKeyName("xci")]
    public bool XCI { get; set; }
    
    [ConfigurationKeyName("nca")]
    public bool NCA { get; set; }
    
    [ConfigurationKeyName("nro")]
    public bool NRO { get; set; }
    
    [ConfigurationKeyName("nso")]
    public bool NSO { get; set; }
}
