using Beer.DaAPI.BlazorApp.ModelHelper;
using Beer.DaAPI.BlazorApp.Validation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorApp.Helper
{
    public static class InjectValidatorExtentions
    {
        public static IRuleBuilderOptions<T, String> InjectIPv4AdressValidator<T>(this IRuleBuilder<T, String> ruleBuilder, String propertyName)
        {
            return ruleBuilder.InjectValidator((prov, ctx) => new IPv4AdressValidator(prov.GetService<IStringLocalizer<ValidationMessagesRessources>>(), propertyName));
        }

        public static IRuleBuilderOptions<T, String> InjectIPv6AdressValidator<T>(this IRuleBuilder<T, String> ruleBuilder,String propertyName)
        {
            return ruleBuilder.InjectValidator((prov, ctx) => new IPv6AdressValidator(prov.GetService<IStringLocalizer<ValidationMessagesRessources>>(), propertyName));
        }
    }
}
