using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorApp.Helper
{
    public class NullableOverrideProperty<T> where T : struct
    {
        public T DefaultValue { get; set; }
        public T? NullableValue { get; set; }
        public T Value { get => NullableValue.Value; }

        private bool _overrideValue = false;

        public Boolean OverrideValue
        {
            get => _overrideValue;
            set
            {
                _overrideValue = value;

                if (value == true)
                {
                    NullableValue = DefaultValue;
                }
                else
                {
                    NullableValue = null;
                }
            }
        }
        public bool HasValue { get => NullableValue.HasValue; }

        public static implicit operator T?(NullableOverrideProperty<T> value) => value.Value;

        internal void UpdateNullableValue(T? value, bool overrideIfNeeded, Func<T?, T?, Boolean> equalityChecker = null)
        {
            if (overrideIfNeeded == true)
            {
                if (value is IEquatable<T> betterEqual)
                {
                    OverrideValue = betterEqual.Equals(DefaultValue) == false;
                }
                else if (equalityChecker != null)
                {
                    OverrideValue = equalityChecker(value, DefaultValue) == false;
                }
                else
                {
                    OverrideValue = value.Equals(DefaultValue);
                }
            }

            NullableValue = value;
        }
    }
}
