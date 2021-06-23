using Beer.DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Core.Scopes
{
    public record IPAddressRange<TAddress> (TAddress Start, TAddress End) where TAddress : IPAddress<TAddress>;
}
