using Beer.DaAPI.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beer.DaAPI.Service.TestHelper
{
    public class MockedAggregateRoot : AggregateRootWithEvents
    {

        public MockedAggregateRoot(IEnumerable<DomainEvent> events) : base(Guid.NewGuid())
        {
            if (events != null)
            {
                foreach (var item in events)
                {
                    Apply(item);
                }
            }
        }
        protected override void When(DomainEvent domainEvent)
        {
        }
    }
}
