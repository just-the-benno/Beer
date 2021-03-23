using Beer.DaAPI.Core.Listeners;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Packets.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Infrastructure.InterfaceEngines
{
    public interface IDHCPv4InterfaceEngine : IDisposable
    {
        public async Task Initialize()
        {
            var savedListeners = await GetActiveListeners();
            foreach (var item in savedListeners)
            {
                OpenListener(item);
            }
        }

        public Task<IEnumerable<DHCPv4Listener>> GetActiveListeners();
        public Boolean OpenListener(DHCPv4Listener listener);
        public Boolean CloseListener(DHCPv4Listener listener);
        IEnumerable<DHCPv4Listener> GetPossibleListeners();

        Boolean SendPacket(DHCPv4Packet packet);
    }
}