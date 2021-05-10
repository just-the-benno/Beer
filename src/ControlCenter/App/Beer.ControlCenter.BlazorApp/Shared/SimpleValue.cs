using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.ControlCenter.BlazorApp.Shared
{
    public class SimpleValue<T>
    {
        public SimpleValue()
        {

        }

        public SimpleValue(T value)
        {
            Value = value;
        }

        public T Value { get; set; }
    }
}
