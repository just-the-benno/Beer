using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.TestHelper
{
    public static class IDictionaryExtention
    {
        public static void AddIfNotExisting<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (dict.ContainsKey(key) == false)
            {
                dict.Add(key, value);
            }
        }

    }
}
