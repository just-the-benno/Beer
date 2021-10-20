using SharpShark.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpShark.Analyzer
{
    public class PacketTrace
    {
        public PacketTrace(IReadOnlyList<int> indicieInFiles)
        {
            IndicieInFiles = indicieInFiles;
            IsTransmitted = indicieInFiles[0] >= 0 && indicieInFiles[^1] >= 0;
        }

        public PacketTrace(IReadOnlyList<int> indicieInFiles, bool orignatedInDestination) : this(indicieInFiles)
        {
            OrignatedInDestination = orignatedInDestination;
        }

        public IReadOnlyList<Int32> IndicieInFiles { get; init; }
        public Boolean IsTransmitted { get; init; }
        public Boolean OrignatedInDestination { get; init; }
    }


    public class PacketTraceFileInfo
    {
        public PacketTraceFileInfo(PcapFile file, int skippedOffset, int abandonOffset) : this(file, (UInt32)skippedOffset, (UInt32)abandonOffset)
        {

        }

        public PacketTraceFileInfo(PcapFile file, uint skippedOffset, uint abandonOffset)
        {
            File = file;
            SkippedOffset = skippedOffset;
            AbandonOffset = abandonOffset;
        }

        public PcapFile File { get; init; }
        public UInt32 SkippedOffset { get; init; }
        public UInt32 AbandonOffset { get; init; }
    }

    public class MPLSPacketLossAnalyzerResult
    {
        public MPLSPacketLossAnalyzerResult(IReadOnlyList<PacketTrace> traces, IReadOnlyList<PacketTraceFileInfo> files)
        {
            Traces = traces;
            Files = files;

            TotalSkipped = files.Max(x => x.SkippedOffset) + (UInt32)files.Select(x => x.File.Packets.Count - x.AbandonOffset).Max();
            TotalLost = (UInt32)traces.Count(x => x.IsTransmitted == false);
            UniquePackets = (UInt32)traces.Count;
        }

        public IReadOnlyList<PacketTrace> Traces { get; init; }
        public IReadOnlyList<PacketTraceFileInfo> Files { get; init; }

        public UInt32 TotalSkipped { get; init; }
        public UInt32 TotalLost { get; init; }
        public UInt32 UniquePackets { get; init; }

        public Int32 GetTracingEntry(Int32 fileIndex, Int32 packetIndexInFile, Boolean shouldBeOrignatedByDestination)
        {
            for (int i = 0; i < Traces.Count; i++)
            {
                var item = Traces[i];

                if (item.IndicieInFiles[fileIndex] == packetIndexInFile && item.OrignatedInDestination == shouldBeOrignatedByDestination)
                {
                    return i;
                }
            }

            return -1;
        }
    }

    public class MPLSAwarePacketLossAnalyzer : IMultipleFileAnalyzer<MPLSPacketLossAnalyzerResult>
    {
        private Boolean _unidirectional;

        public MPLSAwarePacketLossAnalyzer(Boolean unidirectional = false)
        {
            _unidirectional = unidirectional;
        }

        private static void ResetPacketSearchIndicies(IEnumerable<PcapFile> input)
        {
            foreach (var item in input)
            {
                item.Packets.ResetSearchIndex();
            }
        }

        public Int32 FindFirstMatchingPacket(IEnumerable<PcapFile> input)
        {
            var firstFile = input.First();
            var otherThanFirstFile = input.Skip(1).ToArray();
            Int32 result = -1;
            for (int i = 0; i < firstFile.Packets.Count; i++)
            {
                var packet = firstFile.Packets[i];
                Boolean found = true;
                foreach (var file in otherThanFirstFile)
                {
                    Int32 packetIndex = file.Packets.Find<EthernetFrame>(packet, true, PacketCollection.PacketMatchMode.OuterMost);

                    if (packetIndex < 0)
                    {
                        found = false;
                        break;
                    }
                }

                if (found == true)
                {
                    result = i;
                    break;
                }
            }

            ResetPacketSearchIndicies(input);
            return result;
        }

        public Int32 FindLastMatchingPacket(IEnumerable<PcapFile> input, Int32 startIndex)
        {
            //find the file with the least possible packet amount

            PcapFile file = input.First();
            Int32 delta = file.Packets.Count - startIndex;
            foreach (var item in input.Skip(1))
            {
                var tempDelta = item.Packets.Count - startIndex;
                if (tempDelta < delta)
                {
                    delta = tempDelta;
                    file = item;
                }
            }

            return delta - 1;
        }


        public MPLSPacketLossAnalyzerResult Process(IEnumerable<PcapFile> input)
        {
            Int32 fileAmount = input.Count();

            var inputAsArray = input.ToArray();
            var firstFile = inputAsArray[0];
            var lastFile = inputAsArray[^1];

            Int32 startIndex = FindFirstMatchingPacket(input);
            if (startIndex < 0)
            {
                throw new Exception();
            }

            Int32 endIndex = FindLastMatchingPacket(input, startIndex);

            List<PacketTraceFileInfo> files = new(fileAmount);

            foreach (var item in input)
            {
                var startIndexInFile =
                    item.Packets.Find<EthernetFrame>(firstFile.Packets[startIndex], false, PacketCollection.PacketMatchMode.OuterMost);

                var endIndexInFile = startIndexInFile + endIndex;
                files.Add(new PacketTraceFileInfo(item, startIndexInFile, endIndexInFile + 1));
            }

            ResetPacketSearchIndicies(input);


            HashSet<Int32> checkedPackagesInLastFile = new();

            List<PacketTrace> traces = new(endIndex - startIndex);
            for (int j = 0; j <= endIndex; j++)
            {
                var packet = firstFile.Packets[j];

                var indicies = new Int32[fileAmount];
                indicies[0] = j;

                for (int i = 1; i < fileAmount; i++)
                {
                    var file = inputAsArray[i];

                    Int32 packetIndex = file.Packets.Find<EthernetFrame>(packet, true, PacketCollection.PacketMatchMode.OuterMost);
                    if (packetIndex < 0)
                    {
                        file.Packets.MoveIndex(-5);
                        packetIndex = file.Packets.Find<EthernetFrame>(packet, true, PacketCollection.PacketMatchMode.OuterMost);
                    }
                    indicies[i] = packetIndex;

                    if (file == lastFile && packetIndex >= 0)
                    {
                        checkedPackagesInLastFile.Add(packetIndex);
                    }
                }

                traces.Add(new PacketTrace(indicies));
            }

            if (_unidirectional == false)
            {

                ResetPacketSearchIndicies(input);
                var lastPossiblePacketIndexInLastFile = files.Last().AbandonOffset;
                for (Int32 i = (Int32)files.Last().SkippedOffset; i < lastPossiblePacketIndexInLastFile; i++)
                {
                    if (checkedPackagesInLastFile.Contains(i) == true)
                    {
                        continue;
                    }

                    var indicies = new Int32[fileAmount];
                    indicies[0] = -1;
                    indicies[fileAmount - 1] = i;
                    var packet = lastFile.Packets[i];

                    for (int j = 1; j < fileAmount - 1; j++)
                    {
                        var file = inputAsArray[j];
                        Int32 packetIndex = file.Packets.Find<EthernetFrame>(packet, true, PacketCollection.PacketMatchMode.OuterMost);
                        indicies[j] = packetIndex;
                    }

                    traces.Add(new PacketTrace(indicies, true));
                }
            }

            MPLSPacketLossAnalyzerResult result = new(traces.ToArray(), files.ToArray());
            return result;
        }
    }
}
