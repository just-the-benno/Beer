using Beer.DaAPI.Core.Notifications.Triggers;
using Beer.DaAPI.Core.Scopes.DHCPv6;
using Beer.DaAPI.Core.Services;
using Beer.DaAPI.Core.Tracing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Beer.DaAPI.Core.Notifications.Actors
{
    public class NxOsStaticRouteCleanerNotificationActor : NotificationActor
    {
        #region Fields

        private readonly INxOsDeviceConfigurationService _nxosDeviceSerive;
        private readonly DHCPv6RootScope _rootScope;
        private readonly ISerializer _serializer;
        private readonly IDHCPv6PrefixCollector _dhcpv6PrefixCollector;
        private readonly ILogger<NxOsStaticRouteCleanerNotificationActor> _logger;

        #endregion

        #region Properties

        public String Url { get; private set; }
        public String Username { get; private set; }
        public String Password { get; private set; }

        public Boolean IncludesChildren { get; private set; }
        public IEnumerable<Guid> ScopeIds { get; private set; }

        #endregion

        public NxOsStaticRouteCleanerNotificationActor(
            DHCPv6RootScope rootScope,
            ISerializer serializer,
            IDHCPv6PrefixCollector dhcpv6PrefixCollector,
            INxOsDeviceConfigurationService nxosDeviceSerive,
            ILogger<NxOsStaticRouteCleanerNotificationActor> logger
            )
        {
            this._rootScope = rootScope;
            this._serializer = serializer;
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

            var bindings = await _dhcpv6PrefixCollector.GetActiveDHCPv6Prefixes();

            var relevantPrefixes = bindings.Where(x =>
            {
                var scope = _rootScope.GetScopeById(x.Item1);
                if (scope == DHCPv6Scope.NotFound) { return false; }

                if (IsChildOf(scope, ScopeIds) == false) { return false; }

                return true;
            }).Select(x => x.Item2).ToArray();

            await tracingStream.Append(14, TracingRecordStatus.Informative, new Dictionary<String, String>
            {
               { "PrefixCount",  relevantPrefixes.Count().ToString()}
            });


            IEnumerable<PrefixBinding> prefixBindingsToAdd = await _nxosDeviceSerive.CleanupRoutingTable(relevantPrefixes, tracingStream);

            foreach (var item in prefixBindingsToAdd)
            {
                var element = bindings.FirstOrDefault(x =>
                x.Item2.Prefix == item.Prefix && x.Item2.Host == item.Host && x.Item2.Mask == item.Mask);

                if (element == default)
                {
                    continue;
                }

                await _nxosDeviceSerive.AddIPv6StaticRoute(item.Prefix, item.Mask.Identifier, item.Host, tracingStream);
            }

            tracingStream.RevertLevel();

            _logger.LogDebug("actor {name} successfully finished", nameof(NxOsStaticRouteUpdaterNotificationActor));
            return true;
        }

        private bool IsChildOf(DHCPv6Scope scope, IEnumerable<Guid> scopeIds)
        {
            if (scopeIds.Contains(scope.Id) == true) { return true; }

            if (scope.ParentScope == DHCPv6Scope.NotFound) { return false; }

            return IsChildOf(scope.ParentScope, scopeIds);
        }

        public override NotificationActorCreateModel ToCreateModel() => new NotificationActorCreateModel
        {
            Typename = nameof(NxOsStaticRouteCleanerNotificationActor),
            PropertiesAndValues = new Dictionary<String, String>
            {
                {nameof(Url), GetQuotedString(Url)  },
                {nameof(Username), GetQuotedString(Username)  },
                {nameof(Password), GetQuotedString(Password)  },
                { nameof(IncludesChildren), _serializer.Seralize(IncludesChildren) },
                { nameof(ScopeIds), _serializer.Seralize(ScopeIds) },
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

                IncludesChildren = _serializer.Deserialze<Boolean>(propertiesAndValues[nameof(IncludesChildren)]);
                ScopeIds = _serializer.Deserialze<IEnumerable<Guid>>(propertiesAndValues[nameof(ScopeIds)]);

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
            { "Username", Username },
            { "IncludesChildren", JsonSerializer.Serialize(IncludesChildren) },
            { "ScopeIds", JsonSerializer.Serialize(ScopeIds) }
        };
    }
}
