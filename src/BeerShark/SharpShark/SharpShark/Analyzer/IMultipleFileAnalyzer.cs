using SharpShark.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpShark.Analyzer
{
    public interface IMultipleFileAnalyzer<TOutput>
    {
        TOutput Process(IEnumerable<PcapFile> input);

    }
}
