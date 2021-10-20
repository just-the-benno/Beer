using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpShark.Packets
{
    public abstract class PacketStackInfo
    {
        public Packet Packet { get; protected set; }
        public IReadOnlyList<PacketStackInfo> HigherProtocols { get; protected set; } = Array.Empty<PacketStackInfo>();
        public IReadOnlyList<PacketStackInfo> LowerProtocols { get; protected set; } = Array.Empty<PacketStackInfo>();

        public Memory<Byte> Content { get; private set; }

        public UInt16 PaddingLength { get; private set; }

        public PacketStackInfo(Memory<Byte> content, Packet packet)
        {
            Content = content;
            Packet = packet;
        }

        public virtual void SetStack(IEnumerable<PacketStackInfo> lower, IEnumerable<PacketStackInfo> upper)
        {
            LowerProtocols = lower.ToArray();
            HigherProtocols = upper.ToArray();
        }

        public virtual bool Matches(PacketStackInfo otherReleveantStack) => ByteHelper.AreEqual(Content.Span, otherReleveantStack.Content.Span);

        internal void ReadjustLength(UInt16 amountofByteToRemoveFromTheEnd)
        {
            Content = Content.Slice(0, Content.Length - amountofByteToRemoveFromTheEnd);
            PaddingLength = amountofByteToRemoveFromTheEnd;
        }
    }
}
