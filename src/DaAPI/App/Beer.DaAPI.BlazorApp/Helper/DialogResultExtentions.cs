using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorApp.Helper
{
    public static class DialogResultExtentions
    {
        public static Boolean IsSuccess(this DialogResult result) => result.Cancelled == false && result.Data is Boolean value && value == true;
    }
}
