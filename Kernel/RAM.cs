using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel
{
    //TODO: Introducing virtual memory. MMU handles RAM then?
    public class RAM
    {
        private byte[] _pool;

        public RAM(uint size)
        {
            _pool = new byte[size];
        }
    }
}
