using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDPJobManager.Core
{
    public abstract class JobCommand
    {
        public Guid ID { get; set; }
    }
}
