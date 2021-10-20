using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpShark.Packets
{
    public class PacketStack : IReadOnlyList<PacketStackInfo>
    {
        private readonly List<PacketStackInfo> _list;

        public PacketStack()
        {
            _list = new();
        }

        public PacketStack(IEnumerable<PacketStackInfo> stackInfo)
        {
            _list = new List<PacketStackInfo>(stackInfo);
        }

        public IEnumerable<T> GetPacketsOfType<T>(Boolean exactTypeMatch = true) where T : PacketStackInfo => exactTypeMatch == true ? _list.Where(x => x.GetType() == typeof(T)).Cast<T>() : _list.OfType<T>();

        public PacketStackInfo this[int index] => _list[index];

        public int Count => _list.Count;

        public IEnumerator<PacketStackInfo> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
    }
}
