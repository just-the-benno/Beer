using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beer.DaAPI.BlazorApp.ModelHelper
{
    public class TreeItemData : TreeItemData<String>
    {
        public TreeItemData()
        {
            base.Value = String.Empty;
        }

        public new HashSet<TreeItemData> Children { get; init; }
    }

    public class TreeItemData<T> 
    {
        public String Name { get; init; }
        public T Value { get; init; }
        public Boolean IsExpanded { get; set; } = true;

        public HashSet<TreeItemData<T>> Children { get; init; }
    }
}
