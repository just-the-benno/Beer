using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Beer.DaAPI.Infrastructure.InterfaceEngines;
using Beer.DaAPI.Infrastructure.ServiceBus.Messages;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Beer.DaAPI.Infrastructure.ServiceBus.MessageHandler
{
    public class DHCPv4PacketReadyToSendMessageHandler : INotificationHandler<DHCPv4PacketReadyToSendMessage>
    {
        private readonly IDHCPv4InterfaceEngine _engine;
        private readonly ILogger<DHCPv4PacketReadyToSendMessageHandler> _logger;

        public DHCPv4PacketReadyToSendMessageHandler(
            IDHCPv4InterfaceEngine engine,
            ILogger<DHCPv4PacketReadyToSendMessageHandler> logger
            )
        {
            this._engine = engine ?? throw new ArgumentNullException(nameof(engine));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task Handle(DHCPv4PacketReadyToSendMessage notification, CancellationToken cancellationToken)
        {
            _logger.LogDebug("received a DHCPv4PacketReadyToSendMessage from the service bus");
            if (notification.Packet != DHCPv4Packet.Empty)
            {
                _engine.SendPacket(notification.Packet);
            }

            return Task.FromResult(new object());
        }
    }
}
