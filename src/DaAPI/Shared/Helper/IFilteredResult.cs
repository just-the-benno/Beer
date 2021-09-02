using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beer.DaAPI.Shared.Helper
{
    public interface IFilteredResult<T>
    {
        public IEnumerable<T> Result { get; set; }
        public Int32 Total { get; set; }
    }

    public class FilteredResult<T> : IFilteredResult<T>
    {
        public IEnumerable<T> Result { get; set; }
        public Int32 Total { get; set; }

        public FilteredResult()
        {
            Result = Array.Empty<T>();
            Total = 0;
        }

        public FilteredResult(IEnumerable<T> result, Int32 total)
        {
            Result = result;
            Total = total;
        }
    }
}
