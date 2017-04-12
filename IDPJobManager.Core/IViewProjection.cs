using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDPJobManager.Core
{
    public interface IViewProjection<in TIn,out TOut>
    {
        TOut Project(TIn input);
    }
}
