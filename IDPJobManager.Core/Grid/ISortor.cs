using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDPJobManager.Core.Grid
{
    public interface ISortor
    {
        string SortKey { get; set; }
        string SortType { get; set; } 
    }
}
