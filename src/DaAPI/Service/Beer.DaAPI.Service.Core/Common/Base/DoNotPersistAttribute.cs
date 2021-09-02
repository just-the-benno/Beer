using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Core.Common
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true, AllowMultiple = false)]
    sealed class DoNotPersistAttribute : Attribute
    {
        // This is a positional argument
        public DoNotPersistAttribute()
        {
        }
    }
}
