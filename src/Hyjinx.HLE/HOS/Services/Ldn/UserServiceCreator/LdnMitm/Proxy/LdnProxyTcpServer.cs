using NetCoreServer;
using Hyjinx.Common.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;

namespace Hyjinx.HLE.HOS.Services.Ldn.UserServiceCreator.LdnMitm.Proxy
{
    internal partial class LdnProxyTcpServer : TcpServer, ILdnTcpSocket
    {
        private readonly ILogger<LdnProxyTcpServer> _logger = 
            Logger.DefaultLoggerFactory.CreateLogger<LdnProxyTcpServer>();
        
        private readonly LanProtocol _protocol;

        public LdnProxyTcpServer(LanProtocol protocol, IPAddress address, int port) : base(address, port)
        {
            _protocol = protocol;
            OptionReuseAddress = true;
            OptionSendBufferSize = LanProtocol.TcpTxBufferSize;
            OptionReceiveBufferSize = LanProtocol.TcpRxBufferSize;

            _logger.LogInformation(new EventId((int)LogClass.ServiceLdn, nameof(LogClass.ServiceLdn)),
                "LdnProxyTCPServer created a server for this address: {address}:{port}", address, port);
        }

        protected override TcpSession CreateSession()
        {
            return new LdnProxyTcpSession(this, _protocol);
        }

        protected override void OnError(SocketError error)
        {
            LogErrorOccurred(nameof(LdnProxyTcpServer), error);
        }
        
        [LoggerMessage(LogLevel.Error,
            EventId = (int)LogClass.ServiceLdn, EventName = nameof(LogClass.ServiceLdn),
            Message = "{client} caught an error with code {error}")]
        private partial void LogErrorOccurred(string client, SocketError error);

        protected override void Dispose(bool disposingManagedResources)
        {
            Stop();
            base.Dispose(disposingManagedResources);
        }

        public bool Connect()
        {
            throw new InvalidOperationException("Connect was called.");
        }

        public void DisconnectAndStop()
        {
            Stop();
        }

        public bool SendPacketAsync(EndPoint endpoint, byte[] buffer)
        {
            throw new InvalidOperationException("SendPacketAsync was called.");
        }
    }
}
