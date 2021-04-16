using Beer.DaAPI.BlazorApp.Dialogs;
using Beer.DaAPI.BlazorApp.ModelHelper;
using Beer.DaAPI.BlazorApp.Pages.DHCPv6Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Beer.DaAPI.Shared.Requests.DHCPv4InterfaceRequests.V1;
using static Beer.DaAPI.Shared.Responses.DHCPv4InterfaceResponses.V1;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv4Interfaces
{
    public partial class CreateDHCPv4InterfaceDialog : DaAPIDialogBase
    {
        private CreateDHCPv4ListenerViewModel _model;
        private EditForm _form;
        private Boolean _submitInProgress = false;
        private Boolean _hasErrors = false;

        [Parameter] public DHCPv4InterfaceEntry Entry { get; set; }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            if (_model == null)
            {
                _model = new CreateDHCPv4ListenerViewModel
                {
                    InterfaceId = Entry?.PhysicalInterfaceId,
                    IPv4Address = Entry?.IPv4Address,
                    Name = String.Empty,
                };
            }
        }

        public async Task CreateInterface()
        {
            if(_form.EditContext.Validate() == false)
            {
                return;
            }

            _submitInProgress = true;
            _hasErrors = false;

            Boolean result = await _service.CreateDHCPv4Interface(new CreateDHCPv4Listener
            {
                InterfaceId = _model.InterfaceId,
                IPv4Address = _model.IPv4Address,
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
