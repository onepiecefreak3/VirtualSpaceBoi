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
    [KernelMeta(Name = "VirtualSpaceBoi: Kernel")]
    public class Kernel : IKernel
    {
        private Object _queueLock = new Object();

        private SysModuleComposition _sysComp;
        private Queue<SysCallQueueMeta> _sysCallQueue;
        private Dictionary<int, SysCallExecution> _runningSysCalls;
        private Dictionary<string, SysModule> _uuidRegister;

        private IDisplay _display;

        public int Main(params object[] args)
        {
            Initialize();

            //TODO: TEMP TestCases - REMOVE
            StartTests();

            StartMainLoop();

            return 0;
        }

        #region Private methods
        private void Initialize()
        {
            //Initialize hardware
            InitHardware();

            //Preparing syscall stuff
            _sysCallQueue = new Queue<SysCallQueueMeta>();
            _runningSysCalls = new Dictionary<int, SysCallExecution>();
            _uuidRegister = new Dictionary<string, SysModule>();

            //initializing sysmodules
            _sysComp = new SysModuleComposition("sysmodules");
            AddSysCallEvents();

            ListLoadedSysModules();
        }

        private void InitHardware()
        {
            var comp = new HardwareComposition("hardware");
            if (comp.Hardware.Count <= 0)
                throw new KernelPanicException("Hardware is missing");

            foreach (var hw in comp.Hardware)
            {
                if (hw.Value is IDisplay disp)
                {
                    _display = disp;
                    _display.SetFrameBuffer(new byte[776 * 426 * 4]);
                }
            }
        }

        private void StartMainLoop()
        {
            while (true)
            {
                SysCallQueueMeta sysCall = null;

                lock (_queueLock)
                {
                    var queueAny = _sysCallQueue.Any();
                    if (queueAny)
                        sysCall = _sysCallQueue.Dequeue();
                }

                if (sysCall != null)
                {
                    Task.Factory.StartNew(() =>
                    {
                        var ident = sysCall.Sender.GetHashCode();

                        var res = _uuidRegister[_runningSysCalls[ident].Handle].ServiceDispatch(sysCall.Args);

                        _runningSysCalls[ident].Result = res;
                        _runningSysCalls[ident].IsFinished = true;
                    });
                }
            }
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

        private int GetPID(SysModule sender)
        {
            var hash = sender.GetHashCode();
            for (int i = 0; i < _sysComp.SysModules.Count; i++)
                if (hash == _sysComp.SysModules[i].Value.GetHashCode())
                    return i + 1;

            throw new KernelPanicException("No PID registered for module");
        }

        private string GetGuid() => Guid.NewGuid().ToString("N");

        private void StartTests()
        {
            Task.Factory.StartNew(() => TestCase1());
            //Task.Factory.StartNew(() => TestCase2());
        }
        #endregion

        #region TestCases
        private void TestCase1()
        {
            var res = _sysComp.SysModules.FirstOrDefault()?.Value.ServiceDispatch();
            if (res != null) Console.WriteLine($"Kernel called {_sysComp.SysModules.First().Metadata.Name} and got an answer:{Environment.NewLine}" +
                $"{res.Aggregate("", (a, b) => a + b.ToString() + Environment.NewLine)}");

            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        private void TestCase2()
        {
        }
        #endregion

        #region SVC
        private object[] OnSysCall(SysModule sender, int svcId, params object[] args)
        {
            var svc = (ServiceCall)svcId;
            Console.WriteLine($"Syscall {svc.ToString()} executed by {sender.Name}");

            var pid = GetPID(sender);

            switch (svc)
            {
                //Get Handle
                case ServiceCall.svcGetHandle:
                    return GetHandle(sender, args);
                //IPC
                case ServiceCall.svcIpc:
                    return Ipc(sender, args);
                //Free handle
                case ServiceCall.svcFreeHandle:
                    return FreeHandle(sender, args);
                //Check for IPC completion
                case ServiceCall.svcCheckIpcCompletion:
                    return CheckForIpcCompletion(sender, args);
                //Get completed results
                case ServiceCall.svcGetIpcResult:
                    return GetIpcResult(sender, args);
                default:
                    throw new KernelPanicException("Unknown svc");
            }
        }

        private object[] GetHandle(SysModule sender, object[] args)
        {
            if (args.Length <= 0)
                throw new KernelPanicException("Insufficiant arguments");
            if (args[0].GetType() != typeof(string))
                throw new KernelPanicException("Unexpected argument type");

            var port = (string)args[0];

            for (var i = 0; i < _sysComp.SysModules.Count; i++)
            {
                if (_sysComp.SysModules[i].Metadata.Ports.Contains(port))
                {
                    var uuid = GetGuid();
                    _uuidRegister.Add(uuid, _sysComp.SysModules[i].Value);

                    return new object[] { uuid };
                }
            }

            throw new KernelPanicException("Unknown port");
        }

        private object[] Ipc(SysModule sender, object[] args)
        {
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

            var sysCallExec = new SysCallExecution { Handle = handle, IsFinished = false, Result = new object[0] };
            _runningSysCalls.Add(sender.GetHashCode(), sysCallExec);

            lock (_queueLock)
            {
                _sysCallQueue.Enqueue(new SysCallQueueMeta { Sender = sender, Args = args2 });
            }

            return new object[0];
        }

        private object[] FreeHandle(SysModule sender, object[] args)
        {
            if (args.Length <= 0)
                throw new KernelPanicException("Insufficiant arguments");
            if (args[0].GetType() != typeof(string))
                throw new KernelPanicException("Unexpected argument type");

            var handle = (string)args[0];
            if (!_uuidRegister.ContainsKey(handle))
                throw new KernelPanicException("Invalid handle");

            _uuidRegister.Remove(handle);

            return new object[0];
        }

        private object[] CheckForIpcCompletion(SysModule sender, object[] args)
        {
            var ident = sender.GetHashCode();
            if (!_runningSysCalls.ContainsKey(ident))
                throw new KernelPanicException("No running IPCs for this module");

            return new object[] { _runningSysCalls[ident].IsFinished };
        }

        private object[] GetIpcResult(SysModule sender, object[] args)
        {
            var ident = sender.GetHashCode();
            if (!_runningSysCalls.ContainsKey(ident))
                throw new KernelPanicException("No running IPCs for this module");
            if (!_runningSysCalls[ident].IsFinished)
                throw new KernelPanicException("IPC not ready yet");

            var res = _runningSysCalls[ident].Result;
            _runningSysCalls.Remove(ident);

            return res;
        }
        #endregion

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

        private class HardwareComposition
        {
#pragma warning disable 0649, 0169
            [ImportMany(typeof(IHardware))]
            public List<Lazy<IHardware, IHardwareMeta>> Hardware;
#pragma warning restore 0649, 0169

            public HardwareComposition(string path)
            {
                var catalog = new AggregateCatalog();

                var files = Directory.GetFiles(path, "*.dll");
                foreach (var file in files)
                {
                    //TODO: Verify public key with MEF

                    var assembly = Assembly.Load(File.ReadAllBytes(file));
                    catalog.Catalogs.Add(new AssemblyCatalog(assembly));
                }

                var container = new CompositionContainer(catalog);
                container.ComposeParts(this);
            }
        }
    }
}
