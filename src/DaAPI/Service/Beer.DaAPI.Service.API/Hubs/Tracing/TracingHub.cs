using Beer.DaAPI.Shared.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.Service.API.Hubs.Tracing
{
    public class TracingHub : Hub<ITracingClient>
    {
        public async Task Subscribe(String groupname)
        {
            await Groups.AddToGroupAsync(base.Context.ConnectionId, groupname);
        }

        public async Task Unsubscribe(String groupname)
        {
           await Groups.RemoveFromGroupAsync(base.Context.ConnectionId, groupname);
        }
    }
}
