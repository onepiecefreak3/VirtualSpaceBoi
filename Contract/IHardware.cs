using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract
{
    public interface IHardware
    {
    }

    public interface IHardwareMeta
    {
        string Name { get; }
        string MAC { get; }
    }

    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class HardwareMetaAttribute : ExportAttribute
    {
        public string Name { get; set; }
        public string MAC { get; set; }
    }
}
