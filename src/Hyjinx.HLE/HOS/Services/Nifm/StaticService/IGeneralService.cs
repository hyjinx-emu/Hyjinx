using Hyjinx.Common;
using Hyjinx.Logging.Abstractions;
using Hyjinx.Common.Utilities;
using Hyjinx.HLE.HOS.Services.Nifm.StaticService.GeneralService;
using Hyjinx.HLE.HOS.Services.Nifm.StaticService.Types;
using Hyjinx.HLE.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;

namespace Hyjinx.HLE.HOS.Services.Nifm.StaticService
{
    partial class IGeneralService : DisposableIpcService<IGeneralService>
    {
        private readonly GeneralServiceDetail _generalServiceDetail;

        private IPInterfaceProperties _targetPropertiesCache = null;
        private UnicastIPAddressInformation _targetAddressInfoCache = null;
        private string _cacheChosenInterface = null;

        public IGeneralService()
        {
            _generalServiceDetail = new GeneralServiceDetail
            {
                ClientId = GeneralServiceManager.Count,
                IsAnyInternetRequestAccepted = true, // NOTE: Why not accept any internet request?
            };

            NetworkChange.NetworkAddressChanged += LocalInterfaceCacheHandler;

            GeneralServiceManager.Add(_generalServiceDetail);
        }

        [CommandCmif(1)]
        // GetClientId() -> buffer<nn::nifm::ClientId, 0x1a, 4>
        public ResultCode GetClientId(ServiceCtx context)
        {
            ulong position = context.Request.RecvListBuff[0].Position;

            context.Response.PtrBuff[0] = context.Response.PtrBuff[0].WithSize(sizeof(int));

            context.Memory.Write(position, _generalServiceDetail.ClientId);

            return ResultCode.Success;
        }

        [CommandCmif(4)]
        // CreateRequest(u32 version) -> object<nn::nifm::detail::IRequest>
        public ResultCode CreateRequest(ServiceCtx context)
        {
            uint version = context.RequestData.ReadUInt32();

            MakeObject(context, new IRequest(context.Device.System, version));

            // Doesn't occur in our case.
            // return ResultCode.ObjectIsNull;

            // Logger.Stub?.PrintStub(LogClass.ServiceNifm, new { version });

            return ResultCode.Success;
        }

        [CommandCmif(5)]
        // GetCurrentNetworkProfile() -> buffer<nn::nifm::detail::sf::NetworkProfileData, 0x1a, 0x17c>
        public ResultCode GetCurrentNetworkProfile(ServiceCtx context)
        {
            ulong networkProfileDataPosition = context.Request.RecvListBuff[0].Position;

            (IPInterfaceProperties interfaceProperties, UnicastIPAddressInformation unicastAddress) = GetLocalInterface(context);

            if (interfaceProperties == null || unicastAddress == null)
            {
                return ResultCode.NoInternetConnection;
            }

            LogConsoleLocalIpAddress(unicastAddress.Address);

            context.Response.PtrBuff[0] = context.Response.PtrBuff[0].WithSize((uint)Unsafe.SizeOf<NetworkProfileData>());

            NetworkProfileData networkProfile = new()
            {
                Uuid = UInt128Utils.CreateRandom(),
            };

            networkProfile.IpSettingData.IpAddressSetting = new IpAddressSetting(interfaceProperties, unicastAddress);
            networkProfile.IpSettingData.DnsSetting = new DnsSetting(interfaceProperties);

            "HyjinxNetwork"u8.CopyTo(networkProfile.Name.AsSpan());

            context.Memory.Write(networkProfileDataPosition, networkProfile);

            return ResultCode.Success;
        }

        [LoggerMessage(LogLevel.Information,
            EventId = (int)LogClass.ServiceNifm, EventName = nameof(LogClass.ServiceNifm),
            Message = "Console's local IP is {localAddress}")]
        private partial void LogConsoleLocalIpAddress(IPAddress localAddress);

        [CommandCmif(12)]
        // GetCurrentIpAddress() -> nn::nifm::IpV4Address
        public ResultCode GetCurrentIpAddress(ServiceCtx context)
        {
            (_, UnicastIPAddressInformation unicastAddress) = GetLocalInterface(context);

            if (unicastAddress == null)
            {
                return ResultCode.NoInternetConnection;
            }

            context.ResponseData.WriteStruct(new IpV4Address(unicastAddress.Address));

            LogConsoleLocalIpAddress(unicastAddress.Address);

            return ResultCode.Success;
        }

        [CommandCmif(15)]
        // GetCurrentIpConfigInfo() -> (nn::nifm::IpAddressSetting, nn::nifm::DnsSetting)
        public ResultCode GetCurrentIpConfigInfo(ServiceCtx context)
        {
            (IPInterfaceProperties interfaceProperties, UnicastIPAddressInformation unicastAddress) = GetLocalInterface(context);

            if (interfaceProperties == null || unicastAddress == null)
            {
                return ResultCode.NoInternetConnection;
            }

            LogConsoleLocalIpAddress(unicastAddress.Address);

            context.ResponseData.WriteStruct(new IpAddressSetting(interfaceProperties, unicastAddress));
            context.ResponseData.WriteStruct(new DnsSetting(interfaceProperties));

            return ResultCode.Success;
        }

        [CommandCmif(18)]
        // GetInternetConnectionStatus() -> nn::nifm::detail::sf::InternetConnectionStatus
        public ResultCode GetInternetConnectionStatus(ServiceCtx context)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return ResultCode.NoInternetConnection;
            }

            InternetConnectionStatus internetConnectionStatus = new()
            {
                Type = InternetConnectionType.WiFi,
                WifiStrength = 3,
                State = InternetConnectionState.Connected,
            };

            context.ResponseData.WriteStruct(internetConnectionStatus);

            return ResultCode.Success;
        }

        [CommandCmif(21)]
        // IsAnyInternetRequestAccepted(buffer<nn::nifm::ClientId, 0x19, 4>) -> bool
        public ResultCode IsAnyInternetRequestAccepted(ServiceCtx context)
        {
            ulong position = context.Request.PtrBuff[0].Position;
#pragma warning disable IDE0059 // Remove unnecessary value assignment
            ulong size = context.Request.PtrBuff[0].Size;
#pragma warning restore IDE0059

            int clientId = context.Memory.Read<int>(position);

            context.ResponseData.Write(GeneralServiceManager.Get(clientId).IsAnyInternetRequestAccepted);

            return ResultCode.Success;
        }

        private (IPInterfaceProperties, UnicastIPAddressInformation) GetLocalInterface(ServiceCtx context)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return (null, null);
            }

            string chosenInterface = context.Device.Configuration.MultiplayerLanInterfaceId;

            if (_targetPropertiesCache == null || _targetAddressInfoCache == null || _cacheChosenInterface != chosenInterface)
            {
                _cacheChosenInterface = chosenInterface;

                (_targetPropertiesCache, _targetAddressInfoCache) = NetworkHelpers.GetLocalInterface(chosenInterface);
            }

            return (_targetPropertiesCache, _targetAddressInfoCache);
        }

        private void LocalInterfaceCacheHandler(object sender, EventArgs e)
        {
            LogNetworkAddressChanged();

            _targetPropertiesCache = null;
            _targetAddressInfoCache = null;
        }

        [LoggerMessage(LogLevel.Information,
            EventId = (int)LogClass.ServiceNifm, EventName = nameof(LogClass.ServiceNifm),
            Message = "Network address changed, invalidating cached data.")]
        private partial void LogNetworkAddressChanged();

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                NetworkChange.NetworkAddressChanged -= LocalInterfaceCacheHandler;

                GeneralServiceManager.Remove(_generalServiceDetail.ClientId);
            }
        }
    }
}
