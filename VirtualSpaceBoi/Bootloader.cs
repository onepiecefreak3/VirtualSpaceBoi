using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Contract;
using System.IO;

namespace VirtualSpaceBoi
{
    public class Bootloader
    {
        public static void Main(string[] args)
        {
            var comp = new KernelComposition(".");

            bool accepted = false;
            var chosen = -1;
            while (!accepted)
            {
                ListKernels(comp);
                var input = Console.ReadLine();

                if (!int.TryParse(input, out chosen))
                {
                    Console.WriteLine($"Invalid input.");
                    continue;
                }

                if (chosen < 0 || chosen >= comp.Kernels.Count)
                {
                    Console.WriteLine($"Invalid input.");
                    continue;
                }

                accepted = true;
            }

            var ret = comp.Kernels[chosen].Value.Main();
            if (ret != 0) Console.WriteLine($"Kernel panic: 0x{ret:X8}");
            else Console.WriteLine("Kernel died.");
        }

        private static void ListKernels(KernelComposition comp)
        {
            Console.WriteLine("Available Kernels:");
            for (int i = 0; i < comp.Kernels.Count; i++)
                Console.WriteLine($"{i}: {comp.Kernels[i].Metadata.Name}");
        }

        private class KernelComposition
        {
#pragma warning disable 0649, 0169
            [ImportMany(typeof(IKernel))]
            public List<Lazy<IKernel, IKernelMeta>> Kernels;
#pragma warning restore 0649, 0169

            public KernelComposition(string path)
            {
                var catalog = new AggregateCatalog();

                var files = Directory.GetFiles(path, "*.dll");
                foreach (var file in files)
                {
                    //TODO: Verify RSA signature of container

                    var assembly = Assembly.Load(File.ReadAllBytes(file));
                    catalog.Catalogs.Add(new AssemblyCatalog(assembly));
                }

                var container = new CompositionContainer(catalog);
                container.ComposeParts(this);
            }
        }
    }
}
