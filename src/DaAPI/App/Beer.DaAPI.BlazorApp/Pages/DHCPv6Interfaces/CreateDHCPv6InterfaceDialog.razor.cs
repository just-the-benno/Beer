using Beer.DaAPI.BlazorApp.Dialogs;
using Beer.DaAPI.BlazorApp.ModelHelper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Requests.DHCPv6InterfaceRequests.V1;
using static Beer.DaAPI.Shared.Responses.DHCPv6InterfaceResponses.V1;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv6Interfaces
{
    public partial class CreateDHCPv6InterfaceDialog : DaAPIDialogBase
    {
        private CreateDHCPv6ListenerViewModel _model;
        private  EditForm _form;
        private Boolean _submitInProgress = false;
        private Boolean _hasErrors = false;

        [Parameter] public DHCPv6InterfaceEntry Entry { get; set; }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            if (_model == null)
            {
                _model = new CreateDHCPv6ListenerViewModel
                {
                    InterfaceId = Entry?.PhysicalInterfaceId,
                    IPv6Address = Entry?.IPv6Address,
                    Name = String.Empty,
                };
            }
        }

        public async Task CreateInterface()
        {
            if (_form.EditContext.Validate() == false)
            {
                return;
            }

            _submitInProgress = true;
            _hasErrors = false;

            Boolean result = await _service.CreateDHCPv6Interface(new CreateDHCPv6Listener
            {
                InterfaceId = _model.InterfaceId,
                IPv6Address = _model.IPv6Address,
                Name = _model.Name,
            });

            _submitInProgress = false;

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
