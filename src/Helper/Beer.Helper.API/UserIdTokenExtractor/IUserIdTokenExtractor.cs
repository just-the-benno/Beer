using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.Helper.API
{
    public interface IUserIdTokenExtractor
    {
        String GetUserId(Boolean onlySub);
    }
}
