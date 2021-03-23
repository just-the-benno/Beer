using Beer.DaAPI.Core.Scopes;
using Beer.DaAPI.Core.Scopes.DHCPv6.ScopeProperties;
using Beer.TestHelper;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Beer.DaAPI.UnitTests.Core.Scopes.DHCPv6.ScopeProperties
{
    public class DHCPv6NumericValueScopePropertyTester
    {
        [Theory]
        [InlineData(150,UInt32.MaxValue, NumericScopePropertiesValueTypes.UInt32, DHCPv6ScopePropertyType.UInt32)]
        [InlineData(154, UInt16.MaxValue, NumericScopePropertiesValueTypes.UInt16, DHCPv6ScopePropertyType.UInt16)]
        [InlineData(215, Byte.MaxValue, NumericScopePropertiesValueTypes.Byte, DHCPv6ScopePropertyType.Byte)]
        public void Constructor(UInt16 optionIdentifier,
            Int64 value,
            NumericScopePropertiesValueTypes numericType,
            DHCPv6ScopePropertyType valueType)
        {
            var property = new DHCPv6NumericValueScopeProperty(optionIdentifier, value, numericType, valueType);

            Assert.Equal(valueType, property.ValueType);
            Assert.Equal(optionIdentifier, property.OptionIdentifier);
            Assert.Equal(numericType, property.NumericType);
            Assert.Equal(value, property.Value);

            var otherProperty = DHCPv6NumericValueScopeProperty.FromRawValue(optionIdentifier, value.ToString(), numericType);

            Assert.Equal(property, otherProperty);
        }

    }
}
