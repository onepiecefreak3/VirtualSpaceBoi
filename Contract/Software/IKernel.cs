using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Software
{
    public interface IKernel
    {
        int Main(params object[] args);
    }

    public interface IKernelMeta
    {
        string Name { get; }
    }

    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class KernelMetaAttribute : ExportAttribute
    {
        public string Name { get; set; }
    }
}
