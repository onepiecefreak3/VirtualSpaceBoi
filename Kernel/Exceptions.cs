using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel
{
    public class KernelPanicException : Exception
    {
        public KernelPanicException(string message) : base(message)
        {

        }
    }
}
