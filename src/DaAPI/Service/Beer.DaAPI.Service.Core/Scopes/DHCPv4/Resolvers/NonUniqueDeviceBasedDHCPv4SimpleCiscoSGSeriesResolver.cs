using Beer.DaAPI.Core.Common;
using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv4;
using Beer.DaAPI.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Core.Scopes.DHCPv4
{
    public class NonUniqueDeviceBasedDHCPv4SimpleCiscoSGSeriesResolver : DeviceBasedDHCPv4SimpleCiscoSGSeriesResolver
    {
        #region Fields

        #endregion

        #region Properties

        public override bool HasUniqueIdentifier => false;

        #endregion

        #region Constructor

        public NonUniqueDeviceBasedDHCPv4SimpleCiscoSGSeriesResolver(IDeviceService deviceService) : base(deviceService)
        {

        }

        #endregion

        #region Methods

        protected override string GetTypeName() => nameof(NonUniqueDeviceBasedDHCPv4SimpleCiscoSGSeriesResolver);

        #endregion
    }
}
