using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorApp.Services.TracingEnricher
{
    public class NotificationSystemRootTracingEnricher : RootTracingEnricher
    {
        private readonly IStringLocalizer<NotificationSystemRootTracingEnricher> _localizer;

        public NotificationSystemRootTracingEnricher(IStringLocalizer<NotificationSystemRootTracingEnricher> localizer, params ProcedureTracingEnricher[] enrichers) : this(localizer, enrichers.AsEnumerable())
        {

        }

        public NotificationSystemRootTracingEnricher(IStringLocalizer<NotificationSystemRootTracingEnricher> localizer, IEnumerable<ProcedureTracingEnricher> proceudeEnricher) :
            base(10, proceudeEnricher)
        {
            this._localizer = localizer;
        }

        public override string GetModuleIdentifierName() => _localizer["ModuleIdenifier"];
    }
}
