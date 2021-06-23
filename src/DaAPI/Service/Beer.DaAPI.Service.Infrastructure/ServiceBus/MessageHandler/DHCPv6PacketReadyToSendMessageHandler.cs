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
    public class DHCPv6PacketReadyToSendMessageHandler : INotificationHandler<DHCPv6PacketReadyToSendMessage>
    {
        private readonly IDHCPv6InterfaceEngine _engine;
        private readonly ILogger<DHCPv6PacketReadyToSendMessageHandler> _logger;

        public DHCPv6PacketReadyToSendMessageHandler(
            IDHCPv6InterfaceEngine engine,
            ILogger<DHCPv6PacketReadyToSendMessageHandler> logger
            )
        {
            this._engine = engine ?? throw new ArgumentNullException(nameof(engine));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(DHCPv6PacketReadyToSendMessage notification, CancellationToken cancellationToken)
        {
            _logger.LogDebug("received a DHCPv6PacketReadyToSendMessage from the service bus");
            if (notification.Packet != DHCPv6Packet.Empty)
            {
                await _engine.SendPacket(notification.Packet);
            }
        }
    }
}
