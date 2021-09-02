using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorApp.Components
{

    public partial class DaAPILeaseEventHistoryFilter : IDisposable
    {
        private EditContext _context;
        private DaAPILeaseEventHistoryFilterModel _filter;

        [Parameter]
        public EventCallback<DaAPILeaseEventHistoryFilterModel> OnFilterChanged { get; set; }

        protected override void OnInitialized()
        {
            _filter = new ();
            _context = new EditContext(_filter);

            _context.OnFieldChanged += _editContenxt_OnFieldChanged;

            base.OnInitialized();
        }

        private void _editContenxt_OnFieldChanged(object sender, FieldChangedEventArgs e)
        {
            OnFilterChanged.InvokeAsync(_filter);
        }

        public void Dispose()
        {
            _context.OnFieldChanged -= _editContenxt_OnFieldChanged;
        }

    }
}
