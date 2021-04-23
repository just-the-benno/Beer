using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorApp.Helper
{
    public class ValueObjectWithParent<TValue, TParent>
    {
        public ValueObjectWithParent(TValue value, TParent parent)
        {
            Value = value;
            Parent = parent;
        }

        public TValue Value { get; set; }
        public TParent Parent { get; init; }
    }
}
