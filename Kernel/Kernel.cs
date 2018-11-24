using System;
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

        private List<(string uuid, int senderHash, string port)> _uuidRegister;

        public int Main(params object[] args)
        {
            _uuidRegister = new List<(string, int, string)>();

            _sysComp = new SysModuleComposition("sysmodules");
            AddSysCallEvents();

            ListLoadedSysModules();

            var res = _sysComp.SysModules.FirstOrDefault()?.Value.SendSyncRequest();
            if (res != null) Console.WriteLine($"Kernel called {_sysComp.SysModules.First().Metadata.Name} and got an answer:{Environment.NewLine}" +
                $"{res.Aggregate("", (a, b) => a + b.ToString() + Environment.NewLine)}");

            Console.WriteLine("Press any key...");
            Console.ReadKey();

            return 0;
        }

        private void ListLoadedSysModules()
        {
            Console.WriteLine("Loaded sysmodules:");
            foreach (var sys in _sysComp.SysModules)
                Console.WriteLine(sys.Metadata.Name);
            Console.WriteLine();
        }

        private void AddSysCallEvents()
        {
            foreach (var sys in _sysComp.SysModules)
                sys.Value.SysCall += OnSysCall;
        }

        private object[] OnSysCall(SysModule sender, int svcId, params object[] args)
        {
            Console.WriteLine($"Syscall executed by {sender.Name}");
            Console.WriteLine();

            var pid = GetPID(sender);

            switch (svcId)
            {
                //Get Handle
                case 0:
                    if (args.Length <= 0)
                        throw new KernelPanicException("Insufficiant arguments");
                    if (args[0].GetType() != typeof(string))
                        throw new KernelPanicException("Unexpected argument type");

                    var port = (string)args[0];

                    for (var i = 0; i < _sysComp.SysModules.Count; i++)
                    {
                        if (_sysComp.SysModules[i].Metadata.Ports.Contains(port))
                        {
                            var uuid = Guid.NewGuid().ToString("N");
                            _uuidRegister.Add((uuid, _sysComp.SysModules[i].Value.GetHashCode(), port));

                            return new object[] { uuid };
                        }
                    }

                    throw new KernelPanicException("Unknown port");
                    //only executable if uuid handle exists
                case 1:
                    //TODO Implement example
                    return null;
                default:
                    throw new KernelPanicException("Unknown svc");
            }
        }

        private int GetPID(SysModule sender)
        {
            var hash = sender.GetHashCode();
            for (int i = 0; i < _sysComp.SysModules.Count; i++)
                if (hash == _sysComp.SysModules[i].Value.GetHashCode())
                    return i + 1;

            throw new KernelPanicException("No PID registered for module");
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
