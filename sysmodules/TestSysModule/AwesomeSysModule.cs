using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contract.Software;

namespace TestSysModule
{
    [Export(typeof(SysModule))]
    [SysModuleMeta(Name = nameof(AwesomeSysModule), Ports = new[] { "awe:1", "awe:2" })]
    public class AwesomeSysModule : SysModule
    {
        public override string Name => nameof(AwesomeSysModule);

        public override object[] ServiceDispatch(params object[] args)
        {
            Print("Hello World.");

            var awe2Handle = GetHandle("awe2:1");
            SendSyncRequest(awe2Handle, new object[] { $"Value from {nameof(AwesomeSysModule)}", 2000 });
            FreeHandle(awe2Handle);

            //SendSyncRequest(awe2Handle, new object[] { $"Value from {nameof(AwesomeSysModule)}" });

            return new object[0];
        }
    }
}
