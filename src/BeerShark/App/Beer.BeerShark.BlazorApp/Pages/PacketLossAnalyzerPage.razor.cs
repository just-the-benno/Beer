using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using SharpShark;
using SharpShark.Analyzer;
using SharpShark.Packets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beer.BeerShark.BlazorApp.Pages
{
    public class FileInfo
    {
        public String Name { get; set; }
        public Int64 Size { get; set; }
        public Boolean IsReadingInProgress { get; set; } = true;
        public Boolean IsValid { get; set; }
    }

    public partial class PacketLossAnalyzerPage
    {
        private Int32 _activePanelIndex = 0;
        private IList<FileInfo> _files = new List<FileInfo>();
        private IList<PcapFile> _pcapFiles = new List<PcapFile>();
        private MPLSPacketLossAnalyzerResult _result;
        private Boolean _biderectionalRead = true;

        private Boolean _analzying = false;

        private async Task UploadFiles(InputFileChangeEventArgs e)
        {
            foreach (var file in e.GetMultipleFiles())
            {
                var info = new FileInfo
                {
                    Name = file.Name,
                    Size = file.Size,
                };

                _files.Add(info);

                StateHasChanged();

                var stream = file.OpenReadStream(file.Size);
                Byte[] content = new Byte[file.Size];
                await stream.ReadAsync(content, 0, (Int32)file.Size);

                try
                {
                    await Task.Run(() =>
                    {
                        PcapFile pcapFile = PcapFile.FromStream(content, file.Name, PacketStackBuilder.Default);
                        _pcapFiles.Add(pcapFile);
                    });

                    info.IsValid = true;
                }
                catch (Exception)
                {
                    info.IsValid = false;
                }

                info.IsReadingInProgress = false;

            }
        }

        private void MoveFile(FileInfo file, Int32 offset)
        {
            var index = _files.IndexOf(file);
            var pcapFile = _pcapFiles[index];
            var newIndex = index + offset;
            if (offset > 0)
            {
                newIndex++;
            }
            _files.Insert(newIndex, file);
            _pcapFiles.Insert(newIndex, pcapFile);

            _files.RemoveAt(index + (offset < 0 ? +1 : 0));
            _pcapFiles.RemoveAt(index + (offset < 0 ? +1 : 0));

        }

        private void MoveFileUpward(FileInfo file) => MoveFile(file, -1);
        private void MoveFileDownward(FileInfo file) => MoveFile(file, +1);

        private void GoToNextTab() => _activePanelIndex = 1;

        private void StartAnalyze()
        {
            _analzying = true;
            StateHasChanged();

            var analyzer = new MPLSAwarePacketLossAnalyzer(!_biderectionalRead);
            var result = analyzer.Process(_pcapFiles);

            _result = result;

            _analzying = false;
            StateHasChanged();
        }
    }
}
