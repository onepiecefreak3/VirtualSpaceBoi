using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Kernel
{
    public class Kernel
    {
        public int kernel_main(string[] args)
        {
            Console.WriteLine(args[0]);
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
