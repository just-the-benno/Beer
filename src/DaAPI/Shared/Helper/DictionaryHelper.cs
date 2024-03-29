﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Beer.DaAPI.Shared.Helper
{
    public static class DictionaryHelper
    {
        public static IDictionary<String, TValue> NormelizedProperties<TValue>(IDictionary<String, TValue> input)
        {
            var normelizedResolverProperties = new Dictionary<String, TValue>();
            foreach (var property in input)
            {
                String newKey = property.Key;
                if (Char.IsLower(property.Key[0]) == true)
                {
                    newKey = Char.ToUpper(property.Key[0]) + newKey[1..];
                }

                normelizedResolverProperties.Add(newKey, property.Value);
            }

            return normelizedResolverProperties;
        }
    }
}
