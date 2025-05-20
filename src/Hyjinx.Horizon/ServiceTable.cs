using Hyjinx.Horizon.Arp;
using Hyjinx.Horizon.Audio;
using Hyjinx.Horizon.Bcat;
using Hyjinx.Horizon.Friends;
using Hyjinx.Horizon.Hshl;
using Hyjinx.Horizon.Ins;
using Hyjinx.Horizon.Lbl;
using Hyjinx.Horizon.LogManager;
using Hyjinx.Horizon.MmNv;
using Hyjinx.Horizon.Ngc;
using Hyjinx.Horizon.Ovln;
using Hyjinx.Horizon.Prepo;
using Hyjinx.Horizon.Psc;
using Hyjinx.Horizon.Ptm;
using Hyjinx.Horizon.Sdk.Arp;
using Hyjinx.Horizon.Srepo;
using Hyjinx.Horizon.Usb;
using Hyjinx.Horizon.Wlan;
using System.Collections.Generic;
using System.Threading;

namespace Hyjinx.Horizon
{
    public class ServiceTable
    {
        private int _readyServices;
        private int _totalServices;

        private readonly ManualResetEvent _servicesReadyEvent = new(false);

        public IReader ArpReader { get; internal set; }
        public IWriter ArpWriter { get; internal set; }

        public IEnumerable<ServiceEntry> GetServices(HorizonOptions options)
        {
            List<ServiceEntry> entries = new();

            void RegisterService<T>() where T : IService
            {
                entries.Add(new ServiceEntry(T.Main, this, options));
            }

            RegisterService<ArpMain>();
            RegisterService<AudioMain>();
            RegisterService<BcatMain>();
            RegisterService<FriendsMain>();
            RegisterService<HshlMain>();
            RegisterService<HwopusMain>(); // TODO: Merge with audio once we can start multiple threads.
            RegisterService<InsMain>();
            RegisterService<LblMain>();
            RegisterService<LmMain>();
            RegisterService<MmNvMain>();
            RegisterService<NgcMain>();
            RegisterService<OvlnMain>();
            RegisterService<PrepoMain>();
            RegisterService<PscMain>();
            RegisterService<SrepoMain>();
            RegisterService<TsMain>();
            RegisterService<UsbMain>();
            RegisterService<WlanMain>();

            _totalServices = entries.Count;

            return entries;
        }

        internal void SignalServiceReady()
        {
            if (Interlocked.Increment(ref _readyServices) == _totalServices)
            {
                _servicesReadyEvent.Set();
            }
        }

        public void WaitServicesReady()
        {
            _servicesReadyEvent.WaitOne();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _servicesReadyEvent.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}