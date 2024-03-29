﻿using Beer.DaAPI.BlazorApp.Dialogs;
using Beer.DaAPI.BlazorApp.Services;
using Beer.DaAPI.Shared.JsonConverters;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorApp.Pages.DHCPv6Scopes
{
    public partial class DHCPv6ImportScopeStructureDialog : DaAPIDialogBase
    {
        [Inject] DaAPIService Service { get; set; }
        [CascadingParameter] MudDialogInstance Instance { get; set; }

        private String _content;
        private Int32 _numberOfScopes = 0;
        private Int32 _currentScope = 0;
        private Boolean _inProgress = false;

        private Boolean IsInputValid() => IsInputValid(_content);

        private JsonSerializerSettings _jsonSettings;

        public DHCPv6ImportScopeStructureDialog()
        {
            _jsonSettings =
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                };

            _jsonSettings.Converters.Add(new DHCPv6ScopePropertyRequestJsonConverter());
            _jsonSettings.Converters.Add(new DHCPv4ScopePropertyRequestJsonConverter());

        }

        private Boolean IsInputValid(String input)
        {
            try
            {
                var requests = JsonConvert.DeserializeObject<List<DHCPv6ExportRequest>>(input, _jsonSettings);
                return requests.Count > 1;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task StartImport()
        {
            if (IsInputValid() == false) { return; }

            var requests = JsonConvert.DeserializeObject<List<DHCPv6ExportRequest>>(_content, _jsonSettings);

            _numberOfScopes = requests.Count();
            _inProgress = true;
            StateHasChanged();

            Dictionary<Guid, Guid> parentIdMapper = new();

            foreach (var request in requests)
            {
                if (request.Request.ParentId.HasValue == true)
                {
                    request.Request.ParentId = parentIdMapper[request.Request.ParentId.Value];
                }

                var result = await Service.CreateDHCPv6Scope(request.Request);
                _currentScope++;
                StateHasChanged();

                if (result != default)
                {
                    parentIdMapper.Add(request.Id, result);
                }
                else
                {

                }
            }

            Instance.Close(DialogResult.Ok(true));
        }
    }
}
