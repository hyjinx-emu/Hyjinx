namespace Hyjinx.Horizon.Sdk.Sf.Cmif;

abstract partial class DomainServiceObject : ServerDomainBase, IServiceObject
{
    public abstract ServerDomainBase GetServerDomain();
}