using Beer.DaAPI.BlazorApp.Resources;
using Beer.DaAPI.Shared.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv4Scopes
{
    public class SimpleIPv4AddressString
    {
        [Required]
        [IPv4Address(ErrorMessageResourceName = nameof(ValidationErrorMessages.IPv4Address), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        [IsUniqueInCollection(nameof(OtherItems), ErrorMessageResourceName = nameof(ValidationErrorMessages.IsUniqueInCollection), ErrorMessageResourceType = typeof(ValidationErrorMessages))]
        public String Value { get; set; }

        public IEnumerable<SimpleIPv4AddressString> OtherItems { get; }

        public SimpleIPv4AddressString(String content) : this(content, Array.Empty<SimpleIPv4AddressString>())
        {
        }

        public SimpleIPv4AddressString(IEnumerable<SimpleIPv4AddressString> otherItems) : this(String.Empty, otherItems)
        {
        }

        public SimpleIPv4AddressString(String content, IEnumerable<SimpleIPv4AddressString> otherItems)
        {
            Value = content;
            OtherItems = otherItems;
        }
    }
}
