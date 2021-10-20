using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpShark.Packets
{
    public class PacketCollection : IReadOnlyList<Packet>
    {
        public enum PacketMatchMode
        {
            OuterMost,
            InnerMost,
        }

        private const int _itemNotFoundIndex = -1;
        private readonly List<Packet> _list;

        private Int32? _searchPointer = null;

        public Packet this[int index] => _list[index];
        public int Count => _list.Count;
        public IEnumerator<Packet> GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

        public PacketCollection(IEnumerable<Packet> input)
        {
            _list = new(input);
        }

        private Boolean CheckIfPacketMatches<T>(Packet packet, Int32 index, Boolean changeSearchPointer, PacketMatchMode packetMatchMode)
            where T : PacketStackInfo
        {
            var savedPacket = _list[index];

            var ownStackItems = savedPacket.Stack.GetPacketsOfType<T>();
            if (ownStackItems.Any() == false) { return false; }

            var otherStackItems = packet.Stack.GetPacketsOfType<T>();
            if (otherStackItems.Any() == false) { return false; }

            var ownReleveantStack = ownStackItems.First();
            var otherReleveantStack = otherStackItems.First();

            if (packetMatchMode == PacketMatchMode.OuterMost)
            {
                ownReleveantStack = ownStackItems.Last();
                otherReleveantStack = otherStackItems.Last();
            }

            if (ownReleveantStack.Matches(otherReleveantStack) == true)
            {
                if (changeSearchPointer == true)
                {
                    _searchPointer = index + 1;
                }

                return true;
            }

            return false;
        }

        public Int32 Find<T>(Packet packet, Boolean changeSearchPointer, PacketMatchMode packetMatchMode)
                 where T : PacketStackInfo
        {
            for (int i = _searchPointer ?? 0; i < _list.Count; i++)
            {
                if (CheckIfPacketMatches<T>(packet, i, changeSearchPointer, packetMatchMode) == true)
                {
                    return i;
                }
            }

            return _itemNotFoundIndex;
        }

        public int FindLast<T>(Packet packet, Boolean changeSearchPointer, PacketMatchMode packetMatchMode)
                 where T : PacketStackInfo

        {
            for (int i = _searchPointer ?? (_list.Count - 1); i >= 0; i--)
            {
                if (CheckIfPacketMatches<T>(packet, i, changeSearchPointer, packetMatchMode) == true)
                {
                    return i;
                }
            }

            return _itemNotFoundIndex;
        }

        public void ResetSearchIndex() => _searchPointer = null;

        public PacketCollection GetBetween<T>(Packet start, Packet end, PacketMatchMode mode)
            where T : PacketStackInfo
        {
            ResetSearchIndex();
            Int32 startIndex = Find<T>(start, true, mode);
            Int32 endIndex = Find<T>(end, false, mode);

            ResetSearchIndex();

            return new PacketCollection(_list.Skip(startIndex).Take(endIndex - startIndex + 1));
        }

        public PacketCollection Remove<T>(Packet packet, PacketMatchMode mode)
            where T : PacketStackInfo
        {
            Int32 index = Find<T>(packet, false, mode);
            var result = new PacketCollection(_list);
            if(index >= 0)
            {
                result._list.RemoveAt(index);
            }

            return result;
        }

        public void MoveIndex(int offset)
        {
            if(_searchPointer.HasValue == false) { return; }

            _searchPointer = Math.Min(0, _searchPointer.Value - offset);
        }
    }
}
