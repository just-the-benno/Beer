using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpShark.Packets
{
    public class Packet
    {
        public Memory<Byte> Content { get; init; }
        public DateTime Timestamp { get; init; }
        public UInt32 Size { get; init; }
        public UInt32 DatalinkIdentifier { get; init; }

        public PacketStack Stack { get; private set; } = new PacketStack();

        public Packet(DateTime timestamp, UInt32 size, UInt32 datalinkIdentifier,  Byte[] content)
        {
            Timestamp = timestamp;
            Size = size;
            Content = content;
            DatalinkIdentifier = datalinkIdentifier;
        }

        public void BuildStack(IPacketStackBuilder parser)
        {
            var stack = parser.BuildPacketStack(this);
            Stack = new PacketStack(stack);

            for (int i = 0; i < Stack.Count; i++)
            {
                var item = Stack[i];
                item.SetStack(Stack.Take(i).ToList().AsReadOnly(),
                    Stack.Skip(i + 1).ToList().AsReadOnly());
            }
        }
    }
}
