using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDPJobManager.Core.Grid
{
    public interface IPager
    {
        int PageCurrent { get; set; }
        int PageSize { get; set; }
    }
}
