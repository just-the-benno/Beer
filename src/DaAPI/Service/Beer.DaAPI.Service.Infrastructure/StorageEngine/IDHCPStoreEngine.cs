using Beer.DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Service.Infrastructure.StorageEngine
{
    public interface IDHCPStoreEngine<TRootScope>
    {
        Task<TRootScope> GetRootScope();
        Task<T> GetAggregateRoot<T>(Guid id) where T : AggregateRootWithEvents, new();
        Task<Boolean> Save(AggregateRootWithEvents root);
        Task<Boolean> CheckIfAggrerootExists<T>(Guid id) where T : AggregateRootWithEvents, new();
    }
}
