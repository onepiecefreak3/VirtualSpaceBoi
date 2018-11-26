using Contract.Software;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel
{
    internal enum ServiceCall : int
    {
        svcGetHandle,
        svcIpc,
        svcFreeHandle,
        svcCheckIpcCompletion,
        svcGetIpcResult
    }

    internal class SysCallQueueMeta
    {
        public SysModule Sender { get; set; }
        public object[] Args { get; set; }
    }

    internal class SysCallExecution
    {
        public string Handle { get; set; }
        public bool IsFinished { get; set; }
        public object[] Result { get; set; }
    }
}
