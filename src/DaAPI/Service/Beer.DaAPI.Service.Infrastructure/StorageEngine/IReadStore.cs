using Beer.DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Infrastructure.StorageEngine
{
    public interface IReadStore
    {
        Task<Boolean> Project(IEnumerable<DomainEvent> events);
        Task<IEnumerable<Guid>>  GetAllNotificationPipelineIds();
    }
}
