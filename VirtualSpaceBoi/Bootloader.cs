using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VirtualSpaceBoi
{
    public class Bootloader
    {
        public static void Main(string[] args)
        {
            var kernelDll = Assembly.Load("Kernel");
            // Init stuff
            var type = kernelDll.GetType("Kernel.Kernel");
            var method = type.GetMethod("kernel_main");
            method.Invoke(Activator.CreateInstance(type), new object[] { args });
            // Cleanup stuff

            Console.WriteLine("Kernel died");
            Console.ReadLine();
        }
    }
}
