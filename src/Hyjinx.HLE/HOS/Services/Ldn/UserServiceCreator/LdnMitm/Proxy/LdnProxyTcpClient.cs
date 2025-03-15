using Hyjinx.Common.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Hyjinx.HLE.HOS.Services.Ldn.UserServiceCreator.LdnMitm.Proxy
{
    internal partial class LdnProxyTcpClient : NetCoreServer.TcpClient, ILdnTcpSocket
    {
        private readonly ILogger<LdnProxyTcpClient> _logger = 
            Logger.DefaultLoggerFactory.CreateLogger<LdnProxyTcpClient>();
        
        private readonly LanProtocol _protocol;
        private byte[] _buffer;
        private int _bufferEnd;

        public LdnProxyTcpClient(LanProtocol protocol, IPAddress address, int port) : base(address, port)
        {
            _protocol = protocol;
            _buffer = new byte[LanProtocol.BufferSize];
            OptionSendBufferSize = LanProtocol.TcpTxBufferSize;
            OptionReceiveBufferSize = LanProtocol.TcpRxBufferSize;
            OptionSendBufferLimit = LanProtocol.TxBufferSizeMax;
            OptionReceiveBufferLimit = LanProtocol.RxBufferSizeMax;
        }

        protected override void OnConnected()
        {
            LogClientConnected();
        }
        
        [LoggerMessage(LogLevel.Information,
            EventId = (int)LogClass.ServiceLdn, EventName = nameof(LogClass.ServiceLdn),
            Message = "LdnProxyTCPClient connected!")]
        private partial void LogClientConnected();

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            _protocol.Read(ref _buffer, ref _bufferEnd, buffer, (int)offset, (int)size);
        }

        public void DisconnectAndStop()
        {
            DisconnectAsync();

            while (IsConnected)
            {
                Thread.Yield();
            }
        }

        public bool SendPacketAsync(EndPoint endPoint, byte[] data)
        {
            if (endPoint != null)
            {
                Logger.Warning?.PrintMsg(LogClass.ServiceLdn, "LdnProxyTcpClient is sending a packet but endpoint is not null.");
            }

            if (IsConnecting && !IsConnected)
            {
                LogConnectBeforeSendingPackets();

                while (IsConnecting && !IsConnected)
                {
                    Thread.Yield();
                }
            }

            return SendAsync(data);
        }

        [LoggerMessage(LogLevel.Information,
            EventId = (int)LogClass.ServiceLdn, EventName = nameof(LogClass.ServiceLdn),
            Message = "LdnProxyTCPClient needs to connect before sending packets. Waiting...")]
        private partial void LogConnectBeforeSendingPackets();

        protected override void OnError(SocketError error)
        {
            LogErrorOccurred(nameof(LdnProxyTcpClient), error);
        }

        [LoggerMessage(LogLevel.Error,
            EventId = (int)LogClass.ServiceLdn, EventName = nameof(LogClass.ServiceLdn),
            Message = "{client} caught an error with code {error}")]
        private partial void LogErrorOccurred(string client, SocketError error);

        protected override void Dispose(bool disposingManagedResources)
        {
            DisconnectAndStop();
            base.Dispose(disposingManagedResources);
        }

        public override bool Connect()
        {
            // TODO: NetCoreServer has a Connect() method, but it currently leads to weird issues.
            base.ConnectAsync();

            while (IsConnecting)
            {
                Thread.Sleep(1);
            }

            return IsConnected;
        }

        public bool Start()
        {
            throw new InvalidOperationException("Start was called.");
        }

        public bool Stop()
        {
            throw new InvalidOperationException("Stop was called.");
        }
    }
}
