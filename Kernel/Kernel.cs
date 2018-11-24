﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private SysModuleComposition _sysComp;

        public int Main(params object[] args)
        {
            _sysComp = new SysModuleComposition("sysmodules");

            ListLoadedSysModules();

            var res = _sysComp.SysModules.FirstOrDefault()?.Value.SendSyncRequest();
            if (res != null) Console.WriteLine($"Kernel called {_sysComp.SysModules.First().Metadata.Name} and got an answer:{Environment.NewLine}" +
                $"{res.Aggregate("", (a, b) => a + b.ToString() + Environment.NewLine)}");

            Console.WriteLine("Press any key...");
            Console.ReadKey();

            //var ret = InitializeRAM(0x10000000);
            //if (ret != 0) return ret;

            return 0;
        }

        private void ListLoadedSysModules()
        {
            Console.WriteLine("Loaded sysmodules:");
            foreach (var sys in _sysComp.SysModules)
                Console.WriteLine(sys.Metadata.Name);
        }

        private int InitializeRAM(uint size)
        {
            _ram = new RAM(size);

            return 0;
        }

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

        private class SysModuleComposition
        {
#pragma warning disable 0649, 0169
            [ImportMany(typeof(SysModule))]
            public List<Lazy<SysModule, ISysModuleMeta>> SysModules;
#pragma warning restore 0649, 0169

            public SysModuleComposition(string path)
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
