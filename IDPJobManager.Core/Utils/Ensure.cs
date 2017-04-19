using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDPJobManager.Core.Utils
{
    public static class Ensure
    {
        public static void Requires<TException>(bool condition) where TException : Exception,new ()
        {
            if(!condition)
            {
                throw new TException();
            }
        }
    }
}
