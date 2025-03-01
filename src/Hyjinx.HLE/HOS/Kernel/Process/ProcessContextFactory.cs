using Hyjinx.Memory;

namespace Hyjinx.HLE.HOS.Kernel.Process
{
    class ProcessContextFactory : IProcessContextFactory
    {
        public IProcessContext Create(KernelContext context, ulong pid, ulong addressSpaceSize, InvalidAccessHandler invalidAccessHandler, bool for64Bit)
        {
            return new ProcessContext(new AddressSpaceManager(context.Memory, addressSpaceSize), addressSpaceSize);
        }
    }
}
