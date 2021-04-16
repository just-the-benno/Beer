using Beer.DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorApp.Helper
{
    public static class DUIDExtentions
    {
        public static String ToFriendlyString(this DUID input) => input switch
        {
            UUIDDUID duid => duid.UUID.ToString(),
            LinkLayerAddressDUID duid => $"L2: {ByteHelper.ToString(duid.LinkLayerAddress, '-')}",
            LinkLayerAddressAndTimeDUID duid => $"L2: {ByteHelper.ToString(duid.LinkLayerAddress, '-')} | Time: {duid.Time}",
            VendorBasedDUID duid => $"Enterprise: {duid.EnterpriseNumber} | Identifier: {ByteHelper.ToString(duid.Identifier)}",
            _ => String.Empty,
        };
    }
}
