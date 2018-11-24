using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract
{
    public abstract class SysModule
    {
        public delegate object[] SysEventHandler(SysModule sender, int svcId, params object[] args);

        public abstract event SysEventHandler SysCall;

        public abstract string Name { get; }

        public abstract object[] SendSyncRequest(params object[] args);
    }

    public interface ISysModuleMeta
    {
        string Name { get; }
        string[] Ports { get; }
    }

    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SysModuleMetaAttribute : ExportAttribute
    {
        public string Name { get; set; }
        public string[] Ports { get; set; }
    }
}
