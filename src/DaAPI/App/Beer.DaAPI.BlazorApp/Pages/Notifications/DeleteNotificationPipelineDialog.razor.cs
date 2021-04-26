using Beer.DaAPI.BlazorApp.Dialogs;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Responses.NotificationPipelineResponses.V1;

namespace Beer.DaAPI.BlazorApp.Pages.Notifications
{
    public partial class DeleteNotificationPipelineDialog : DaAPIDialogBase
    {
        private Boolean _sendingInProgress = false;
        private Boolean _hasErrors = false;

        [Parameter] public NotificationPipelineReadModel Entry { get; set; }

        private async Task SendDeleteRequest()
        {
            _sendingInProgress = true;
            _hasErrors = false;

            Boolean result = await _service.SendDeleteNotificationPipelineRequest(Entry.Id);
            _hasErrors = !result;

            _sendingInProgress = false;

            if (result == true)
            {
                MudDialog.Close(DialogResult.Ok<Boolean>(result));
            }
            else
            {
                _hasErrors = true;
            }
        }
    }
}