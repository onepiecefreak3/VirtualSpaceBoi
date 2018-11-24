using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contract;

namespace TestSysModule
{
    [Export(typeof(SysModule))]
    [SysModuleMeta(Name = nameof(AwesomeSysModule), Ports = new[] { "awe:1", "awe:2" })]
    public class AwesomeSysModule : SysModule
    {
        public override string Name => nameof(AwesomeSysModule);

        public override event SysEventHandler SysCall;

        public override object[] ServiceDispatch(params object[] args)
        {
            Print("Hello World.");

            var awe2Handle = GetHandle("awe2:1");
            SendSyncRequest(awe2Handle, new object[] { $"Value from {nameof(AwesomeSysModule)}" });

            return new object[0];
        }

        private void Print(string message) => Console.WriteLine($"{nameof(AwesomeSysModule)}: {message}");

        private string GetHandle(string port) => (string)SysCall(this, 0, new object[] { port }).First();
        private object[] SendSyncRequest(string handle, params object[] args)
        {
            var args2 = new List<object>();
            args2.Add(handle);
            args2.AddRange(args);
            return SysCall(this, 1, args2.ToArray());
        }
    }
}
