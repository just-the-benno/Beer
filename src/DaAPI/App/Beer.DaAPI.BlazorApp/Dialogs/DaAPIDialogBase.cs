using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorApp.Dialogs
{
    public abstract class DaAPIDialogBase : ComponentBase
    {
        [CascadingParameter] public MudDialogInstance MudDialog { get; set; }

        public void Submit() => MudDialog.Close(DialogResult.Ok(true));
        public void Cancel() => MudDialog.Cancel();
    }
}
