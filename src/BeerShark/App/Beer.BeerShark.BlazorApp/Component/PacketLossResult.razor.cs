using Microsoft.AspNetCore.Components;
using SharpShark.Analyzer;
using System;
using MudBlazor.Utilities;

namespace Beer.BeerShark.BlazorApp.Component
{
    public partial class PacketLossResult
    {
        private Int32 _pageIndex = 1;
        private const Int32 _itemsPerPage = 100;

        private String _columnWidth;

        private String GetColumnWidth() => _columnWidth ??= new StyleBuilder("width", $"{(100.0 / Result.Files.Count)}%").Build();

        [Parameter] public MPLSPacketLossAnalyzerResult Result { get; set; }


    }
}
