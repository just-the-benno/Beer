using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Responses.DHCPv6ScopeResponses.V1;
using static Beer.DaAPI.Shared.Responses.StatisticsControllerResponses.V1;

namespace Beer.DaAPI.BlazorApp.Pages.Dashboard
{
    public class DHCPv6PacketHandledEntryViewModel : DHCPv6PacketHandledEntry, IPacketEntry
    {
        public DHCPv6ScopeItem Scope { get; set; }

        public Int32 RequestSize => base.Request.Content.Length;
        public String RequestSourceAddress => base.Request.Header.Source;
        public Boolean IsSucessResult => base.ResultCode == 0;
        public Int32 ErrorCode => base.ResultCode;

        public Int32? ResponseSize => base.Response?.Content?.Length;

        String IPacketEntry.RequestType => base.RequestType.ToString();
        String IPacketEntry.ResponseType => base.ResponseType.HasValue == false ? String.Empty : base.ResponseType.ToString();
    }
}
