using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Contract;

namespace TestSysModule2
{
    [Export(typeof(SysModule))]
    [SysModuleMeta(Name = nameof(AwesomeSysModule2), Ports = new[] { "awe2:1", "awe2:2" })]
    public class AwesomeSysModule2 : SysModule
    {
        public override string Name => nameof(AwesomeSysModule2);

        public override object[] ServiceDispatch(params object[] args)
        {
            Print("Called with a valid handle");
            Print((string)args[0]);

            Thread.Sleep((int)args[1]);

            return new object[1] { $"Hi there from {nameof(AwesomeSysModule2)}" };
        }
    }
}
