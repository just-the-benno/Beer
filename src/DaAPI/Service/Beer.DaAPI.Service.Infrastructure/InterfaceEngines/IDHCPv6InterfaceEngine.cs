using Beer.DaAPI.Core.Listeners;
using Beer.DaAPI.Core.Packets.DHCPv6;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Infrastructure.InterfaceEngines
{
    public interface IDHCPv6InterfaceEngine : IDisposable
    {
        public async Task Initialize()
        {
            var savedListeners = await GetActiveListeners();
            foreach (var item in savedListeners)
            {
                OpenListener(item);
            }
        }

        public Task<IEnumerable<DHCPv6Listener>> GetActiveListeners();
        public Boolean OpenListener(DHCPv6Listener listener);
        public Boolean CloseListener(DHCPv6Listener listener);
        IEnumerable<DHCPv6Listener> GetPossibleListeners();

        Task<Boolean> SendPacket(DHCPv6Packet packet);
    }
}