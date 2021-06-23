using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Infrastructure.ServiceBus.Messages
{
    public record UnableToSentPacketMessage(String Address) : IMessage;

}
