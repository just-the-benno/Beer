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
    public class NxOsStaticRouteUpdaterNotificationActor : NotificationActor
    {
        #region Fields

        private readonly INxOsDeviceConfigurationService _nxosDeviceSerive;
        private readonly ILogger<NxOsStaticRouteUpdaterNotificationActor> _logger;

        #endregion

        #region Properties

        public String Url { get; private set; }
        public String Username { get; private set; }
        public String Password { get; private set; }

        #endregion

        public NxOsStaticRouteUpdaterNotificationActor(
            INxOsDeviceConfigurationService nxosDeviceSerive,
            ILogger<NxOsStaticRouteUpdaterNotificationActor> logger
            )
        {
            this._nxosDeviceSerive = nxosDeviceSerive ?? throw new ArgumentNullException(nameof(nxosDeviceSerive));
            this._logger = logger;
        }

        protected internal override async Task<Boolean> Handle(NotifcationTrigger trigger, TracingStream tracingStream)
        {
            if (trigger is PrefixEdgeRouterBindingUpdatedTrigger == false)
            {
                _logger.LogError("notification actor {name} has an invalid trigger. expected trigger type is {expectedType} actual is {type}",
                    nameof(NxOsStaticRouteUpdaterNotificationActor), typeof(PrefixEdgeRouterBindingUpdatedTrigger), trigger.GetType());

                return false;
            }

            _logger.LogDebug("connection to nx os device {address}", Url);

            await tracingStream.Append(1, new Dictionary<String, String>(){
                { "Url", Url }
                });

            tracingStream.OpenNextLevel(1);

            Boolean isReachabled = await _nxosDeviceSerive.Connect(Url, Username, Password, tracingStream);
            tracingStream.RevertLevel();

            if (isReachabled == false)
            {
                await tracingStream.Append(2, new Dictionary<String, String>(){
                { "Url", Url }
                });

                _logger.LogDebug("unable to connect to device {address}", Url);
                return false;
            }

            var castedTrigger = (PrefixEdgeRouterBindingUpdatedTrigger)trigger;
            _logger.LogDebug("connection to device {address} established", Url);
            if (castedTrigger.OldBinding != null)
            {
                var info = new Dictionary<String, String>(){
                    { "Url", Url },
                { "OldBinding", JsonSerializer.Serialize(new { Host = castedTrigger.OldBinding.Host.ToString(), Network = castedTrigger.OldBinding.Prefix.ToString(), Mask = castedTrigger.OldBinding.Mask.Identifier.ToString() }) }
                };

                await tracingStream.Append(3, info);
                tracingStream.OpenNextLevel(3);
                tracingStream.OpenNextLevel(_nxosDeviceSerive.GetTracingIdenfier());

                Boolean removeResult = await _nxosDeviceSerive.RemoveIPv6StaticRoute(
                    castedTrigger.OldBinding.Prefix, castedTrigger.OldBinding.Mask.Identifier, castedTrigger.OldBinding.Host, tracingStream);

                tracingStream.RevertLevel();
                tracingStream.RevertLevel();

                if (removeResult == false)
                {
                    await tracingStream.Append(4, info);
                    _logger.LogError("unable to remve old route form device {address}. Cancel actor", Url);
                    return false;
                }

                await tracingStream.Append(5, info);
                _logger.LogDebug("static route {prefix}/{mask} via {host} has been removed from {device}",
                     castedTrigger.OldBinding.Prefix, castedTrigger.OldBinding.Mask.Identifier, castedTrigger.OldBinding.Host, Url);
            }

            if (castedTrigger.NewBinding != null)
            {
                var info = new Dictionary<String, String>(){
                    { "Url", Url },
                    { "NewBinding", JsonSerializer.Serialize(new { Host = castedTrigger.NewBinding.Host.ToString(), Network = castedTrigger.NewBinding.Prefix.ToString(), Mask = castedTrigger.NewBinding.Mask.Identifier.ToString() }) }
                };

                await tracingStream.Append(6, info);
                tracingStream.OpenNextLevel(6);
                tracingStream.OpenNextLevel(_nxosDeviceSerive.GetTracingIdenfier());

                Boolean addResult = await _nxosDeviceSerive.AddIPv6StaticRoute(
                 castedTrigger.NewBinding.Prefix, castedTrigger.NewBinding.Mask.Identifier, castedTrigger.NewBinding.Host, tracingStream);

                tracingStream.RevertLevel();
                tracingStream.RevertLevel();

                if (addResult == false)
                {
                    await tracingStream.Append(7, info);

                    _logger.LogError("unable to add a static route to device {address}. Cancel actor", Url);
                    return false;
                }

                await tracingStream.Append(8, info);

                _logger.LogDebug("static route {prefix}/{mask} via {host} has been added from {device}",
                     castedTrigger.NewBinding.Prefix, castedTrigger.NewBinding.Mask.Identifier, castedTrigger.NewBinding.Host, Url);
            }

            await tracingStream.Append(9);

            _logger.LogDebug("actor {name} successfully finished", nameof(NxOsStaticRouteUpdaterNotificationActor));
            return true;
        }

        public override NotificationActorCreateModel ToCreateModel() => new NotificationActorCreateModel
        {
            Typename = nameof(NxOsStaticRouteUpdaterNotificationActor),
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
            { "Url", Url },
            { "Username", Username }
        };
    }
}
