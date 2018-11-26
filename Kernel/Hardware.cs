using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using Contract.Hardware;
using System.Threading;
using System.Windows.Forms;

namespace Kernel
{
    internal class Hardware : IDisposable
    {
        public IDisplay Display { get; private set; }
        public Thread DisplayThread { get; private set; }
        public IMemory Memory { get; private set; }
        public ISecurity Security { get; private set; }

        private string _path;

        public Hardware(string path)
        {
            _path = path;

            Init();
        }

        private void Init()
        {
            InitDisplay();
        }

        private void InitDisplay()
        {
            var displayType = LoadAssemblyType<IDisplay>(Path.Combine(_path, "Display.dll"));

            DisplayThread = new Thread(() =>
            {
                Display = (IDisplay)Activator.CreateInstance(displayType);
                Application.Run((Form)Display);
            });
            DisplayThread.Start();

            while (Display == null)
                ;

            Display.Kill += OnKill;
        }

        private void OnKill(IHardware sender)
        {
            Dispose();

            throw new KernelPanicException($"{sender.Name} got killed");
        }

        private Type LoadAssemblyType<T>(string file)
        {
            //TODO: RSA signature check

            var assemb = Assembly.Load(File.ReadAllBytes(file));
            var loaded = assemb.DefinedTypes.FirstOrDefault(x => x.GetInterfaces().Contains(typeof(T)));
            if (loaded != null)
                return loaded.AsType();

            throw new KernelPanicException("Hardware not found");
        }

        public void Dispose()
        {
            Display = null;
            Memory = null;
            Security = null;
        }
    }
}
