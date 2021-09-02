using Beer.DaAPI.BlazorApp.Helper;
using Beer.DaAPI.BlazorApp.Services;
using Beer.DaAPI.Core.Packets.DHCPv4;
using Beer.DaAPI.Core.Packets.DHCPv6;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Responses.DHCPv4ScopeResponses.V1;
using static Beer.DaAPI.Shared.Responses.DHCPv6ScopeResponses.V1;

namespace Beer.DaAPI.BlazorApp.Pages.PacketMonitor
{
    public enum PacketFilterMode
    {
        Both,
        OnlyIPv4,
        OnlyIPv6
    }

    public class PacketFilterModel
    {
        public PacketFilterMode FilterMode { get; set; }

        public DateTime? From { get; set; }
        public TimeSpan? FromTime { get; set; }
        public DateTime? To { get; set; }
        public TimeSpan? ToTime { get; set; }
        public String MacAddress { get; set; }
        public String RequestedIp { get; set; }
        public String RequestedPrefix { get; set; }
        public String LeasedIp { get; set; }
        public String LeasedPrefix { get; set; }
        public Boolean? Filtered { get; set; }
        public Boolean? Invalid { get; set; }

        public String SourceAddress { get; set; }
        public String DestinationAddress { get; set; }
       
        public Guid? ScopeId { get; set; }
        public Boolean? IncludeScopeChildren { get; set; }
        public DHCPv4MessagesTypes? DHCPv4RequestMessageType { get; set; }
        public DHCPv4MessagesTypes? DHCPv4ResponseMessageType { get; set; }
        public DHCPv6PacketTypes? DHCPv6RequestMessageType { get; set; }
        public DHCPv6PacketTypes? DHCPv6ResponseMessageType { get; set; }
        public Boolean? HasAnswer { get; set; }
        public Int32? ResultCode { get; set; }
    }

    public partial class PacketFilter : IDisposable
    {
        private EditContext _context;
        private PacketFilterModel _filter;

        private IEnumerable<(Int32 Depth, DHCPv4ScopeTreeViewItem Scope)> _dhcpv4Scopes;
        private IEnumerable<(Int32 Depth, DHCPv6ScopeTreeViewItem Scope)> _dhcpv6Scopes;

        [Inject] public DHCPScopeHelper ScopeHelper { get; set; }
        [Inject] public DaAPIService DaAPIService { get; set; }

        [Parameter]
        public EventCallback<PacketFilterModel> OnFilterChanged { get; set; }

        protected override void OnInitialized()
        {
            _filter = new PacketFilterModel();
            _context = new EditContext(_filter);

            _context.OnFieldChanged += _editContenxt_OnFieldChanged;

            base.OnInitialized();
        }

        private void _editContenxt_OnFieldChanged(object sender, FieldChangedEventArgs e)
        {
            OnFilterChanged.InvokeAsync(_filter);
        }

        public void Dispose()
        {
            _context.OnFieldChanged -= _editContenxt_OnFieldChanged;
        }

        protected override async Task OnInitializedAsync()
        {
            _dhcpv4Scopes = await ScopeHelper.GetDHCPv4scopeAsListWithDepth(DaAPIService);
            _dhcpv6Scopes = await ScopeHelper.GetDHCPv6scopeAsListWithDepth(DaAPIService);
        }
    }
}
