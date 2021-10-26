using Beer.DaAPI.Core.Notifications.Triggers;
using Beer.DaAPI.Core.Services;
using Beer.DaAPI.Core.Tracing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Beer.DaAPI.Core.Notifications.Actors
{
    public class NxOsStaticRouteCleanerNotificationActor : NotificationActor
    {
        #region Fields

        private readonly INxOsDeviceConfigurationService _nxosDeviceSerive;
        private readonly IDHCPv6PrefixCollector _dhcpv6PrefixCollector;
        private readonly ILogger<NxOsStaticRouteCleanerNotificationActor> _logger;

        #endregion

        #region Properties

        public String Url { get; private set; }
        public String Username { get; private set; }
        public String Password { get; private set; }

        #endregion

        public NxOsStaticRouteCleanerNotificationActor(
            IDHCPv6PrefixCollector dhcpv6PrefixCollector,
            INxOsDeviceConfigurationService nxosDeviceSerive,
            ILogger<NxOsStaticRouteCleanerNotificationActor> logger
            )
        {
            this._dhcpv6PrefixCollector = dhcpv6PrefixCollector ?? throw new ArgumentNullException(nameof(dhcpv6PrefixCollector));
            this._nxosDeviceSerive = nxosDeviceSerive ?? throw new ArgumentNullException(nameof(nxosDeviceSerive));
            this._logger = logger;
        }

        protected internal override async Task<Boolean> Handle(NotifcationTrigger trigger, TracingStream tracingStream)
        {
            _logger.LogDebug("connection to nx os device {address}", Url);

            await tracingStream.Append(1, TracingRecordStatus.Informative, new Dictionary<String, String>(){
                { "Url", Url }
                });

            tracingStream.OpenNextLevel(1);

            Boolean isReachabled = await _nxosDeviceSerive.Connect(Url, Username, Password, tracingStream);
            tracingStream.RevertLevel();

            if (isReachabled == false)
            {
                await tracingStream.Append(2, TracingRecordStatus.Informative, new Dictionary<String, String>(){
                { "Url", Url }
                });

                _logger.LogDebug("unable to connect to device {address}", Url);
                return false;
            }

            await tracingStream.Append(4, TracingRecordStatus.Informative);
            tracingStream.OpenNextLevel(1);

            IEnumerable<PrefixBinding> bindings = await _dhcpv6PrefixCollector.GetActiveDHCPv6Prefixes();
            await _nxosDeviceSerive.CleanupRoutingTable(bindings,tracingStream);
            tracingStream.RevertLevel();

            _logger.LogDebug("actor {name} successfully finished", nameof(NxOsStaticRouteUpdaterNotificationActor));
            return true;
        }

        public override NotificationActorCreateModel ToCreateModel() => new NotificationActorCreateModel
        {
            Typename = nameof(NxOsStaticRouteCleanerNotificationActor),
            PropertiesAndValues = new Dictionary<String, String>
            {
                {nameof(Url), GetQuotedString(Url)  },
                {nameof(Username), GetQuotedString(Username)  },
                {nameof(Password), GetQuotedString(Password)  },
            }
        };

        public override Boolean ApplyValues(IDictionary<String, String> propertiesAndValues)
        {
            try
            {
                var url = GetValueWithoutQuota(propertiesAndValues[nameof(Url)]);
                var uri = new Uri(url);

                if (uri.Scheme != "http" && uri.Scheme != "https")
                {
                    return false;
                }

                Url = url;

                Username = GetValueWithoutQuota(propertiesAndValues[nameof(Username)]);
                Password = GetValueWithoutQuota(propertiesAndValues[nameof(Password)]);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override int GetTracingIdentifier() => NotificationActor.NxOsStaticRouteUpdaterNotificationTraceIdenifier;

        public override IDictionary<string, string> GetTracingRecordDetails() => new Dictionary<String, String>
        {
            { "Name", nameof(NxOsStaticRouteCleanerNotificationActor) },
            { "Url", Url },
            { "Username", Username }
        };
    }
}
