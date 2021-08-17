using Beer.DaAPI.Core.Tracing;
using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.Service.API.Hubs.Tracing
{
    public static class TracingStreamExtentions
    {
        public static IEnumerable<String> GetHubGroups(
            Int32 systemIdentifier, Int32 procedureIdentifier, Guid? entityId)
        {
            var groups = new List<String>
            {
                $"{systemIdentifier}.{procedureIdentifier}",
                $"{systemIdentifier}.*",
            };

            if (entityId.HasValue == true)
            {
                groups.Add(entityId.Value.ToString());
            }

            return groups;
        }


        public static IEnumerable<String> GetHubGroups(this TracingRecord record)
        {
            var parts = record.Identifier.Split('.');
            Int32 systemIdentifier = Int32.Parse(parts[0]);
            Int32 procedureIdentifier = Int32.Parse(parts[0]);

            return GetHubGroups(systemIdentifier, procedureIdentifier, record.EntityId);
        }

        public static IEnumerable<String> GetHubGroups(this TracingStream stream) =>
            GetHubGroups(stream.SystemIdentifier, stream.ProcedureIdentifier, stream.Record.FirstOrDefault()?.EntityId);
    }
}
