using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using static Beer.Identity.Shared.Responses.LocalUsersResponses.V1;

namespace Beer.Identity.IntegrationTests
{
    public class LocalUserOverviewEqualityComparer : IEqualityComparer<LocalUserOverview>
    {
        public bool Equals(LocalUserOverview x, LocalUserOverview y) =>
            x.Id == y.Id && x.LoginName == y.LoginName && x.DisplayName == y.DisplayName;

        public int GetHashCode([DisallowNull] LocalUserOverview obj) => obj.Id.GetHashCode();
    }
}
