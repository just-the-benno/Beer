using Beer.DaAPI.BlazorApp.Resources;
using Beer.DaAPI.Shared.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv6Scopes
{
    public class SimpleIPv6AddressString
    {
        [Required]
        [IPv6Address(ErrorMessageResourceName = nameof(ValidationErrorMessages.IPv6Address), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [IsUniqueInCollection(nameof(OtherItems), ErrorMessageResourceName = nameof(ValidationErrorMessages.IsUniqueInCollection), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        public String Value { get; set; }

        public IEnumerable<SimpleIPv6AddressString> OtherItems { get; }

        public SimpleIPv6AddressString(IEnumerable<SimpleIPv6AddressString> otherItems)
        {
            OtherItems = otherItems;
        }
    }
}
