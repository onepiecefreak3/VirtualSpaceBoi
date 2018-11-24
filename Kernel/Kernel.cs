using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Contract;

namespace Kernel
{
    [Export(typeof(IKernel))]
    [KernelMeta(Name = "VirtualSpaceBoi - Kernel")]
    public class Kernel : IKernel
    {
        private RAM _ram;

        public int kernel_main(string[] args)
        {
            Console.WriteLine(args[0]);
            return 0;
        }

        public int Main(params object[] args)
        {
            var ret = InitializeRAM(0x10000000);
            if (ret != 0) return ret;

            return 0;
        }

        private int InitializeRAM(uint size)
        {
            _ram = new RAM(size);

            return 0;
        }

        internal void test()
        {
            Console.WriteLine("NONONO");
        }

        private void test2()
        {
            Console.WriteLine("NONONO");
        }
    }
}
