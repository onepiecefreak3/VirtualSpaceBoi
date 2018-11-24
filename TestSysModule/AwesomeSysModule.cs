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
        public override event SysEventHandler SysCall;

        public override object[] SendSyncRequest(params object[] args)
        {
            return new object[1] { $"Hello World from {nameof(AwesomeSysModule)}" };
        }
    }
}
