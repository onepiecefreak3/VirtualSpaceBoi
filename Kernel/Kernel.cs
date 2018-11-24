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
        private class SysCallQueueMeta
        {
            public SysModule Sender { get; set; }
            public int SvcId { get; set; }
            public object[] Args { get; set; }
        }

        private SysModuleComposition _sysComp;
        private Queue<SysCallQueueMeta> _sysCallQueue;
        private Dictionary<string, SysModule> _uuidRegister;

        public int Main(params object[] args)
        {
            _sysCallQueue = new Queue<SysCallQueueMeta>();
            _uuidRegister = new Dictionary<string, SysModule>();

            _sysComp = new SysModuleComposition("sysmodules");
            AddSysCallEvents();

            ListLoadedSysModules();

            Task.Factory.StartNew(() => TestCase());

            while (true)
            {
                if (_sysCallQueue.Any())
                {
                    var sysCall = _sysCallQueue.Dequeue();
                    Task.Factory.StartNew(() => ServiceDispatch(sysCall.Sender, sysCall.SvcId, sysCall.Args));
                }
            }
        }

        private void TestCase()
        {
            var res = _sysComp.SysModules.FirstOrDefault()?.Value.ServiceDispatch();
            if (res != null) Console.WriteLine($"Kernel called {_sysComp.SysModules.First().Metadata.Name} and got an answer:{Environment.NewLine}" +
                $"{res.Aggregate("", (a, b) => a + b.ToString() + Environment.NewLine)}");

            Console.WriteLine("Press any key...");
            Console.ReadKey();
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
            _sysCallQueue.Enqueue(new SysCallQueueMeta { Sender = sender, SvcId = svcId, Args = args });

            return new object[0];
        }

        private object[] ServiceDispatch(SysModule sender, int svcId, params object[] args)
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
                            _uuidRegister.Add(uuid, _sysComp.SysModules[i].Value);

                            return new object[] { uuid };
                        }
                    }

                    throw new KernelPanicException("Unknown port");
                //IPC
                case 1:
                    if (args.Length <= 0)
                        throw new KernelPanicException("Insufficiant arguments");
                    if (args[0].GetType() != typeof(string))
                        throw new KernelPanicException("Unexpected argument type");

                    var handle = (string)args[0];
                    if (!_uuidRegister.ContainsKey(handle))
                        throw new KernelPanicException("Invalid handle");

                    var args2 = new object[args.Length - 1];
                    for (int i = 0; i < args2.Length; i++)
                        args2[i] = args[i + 1];
                    return _uuidRegister[handle].ServiceDispatch(args2);
                //Free handle
                case 2:
                    if (args.Length <= 0)
                        throw new KernelPanicException("Insufficiant arguments");
                    if (args[0].GetType() != typeof(string))
                        throw new KernelPanicException("Unexpected argument type");

                    handle = (string)args[0];
                    if (!_uuidRegister.ContainsKey(handle))
                        throw new KernelPanicException("Invalid handle");

                    _uuidRegister.Remove(handle);

                    return new object[0];
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
