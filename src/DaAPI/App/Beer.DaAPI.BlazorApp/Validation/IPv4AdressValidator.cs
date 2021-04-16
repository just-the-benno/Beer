using Beer.DaAPI.BlazorApp.ModelHelper;
using Beer.DaAPI.Core.Common;
using FluentValidation;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorApp.Validation
{
    public class IPv4AdressValidator : AbstractValidator<String>
    {
        public IPv4AdressValidator(IStringLocalizer<ValidationMessagesRessources> localizer, String propertyName)
        {
            if (localizer is null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }

            RuleFor(x => x).NotEmpty().Must(x =>
            {
               if (x == null) { return false; }

               try
               {
                   IPv4Address.FromString(x);
                   return true;
               }
               catch (Exception)
               {
                   return false;
               }
           }).WithMessage(localizer["IPv4Address_NotValid"]).WithName(propertyName);
        }
    }
}
