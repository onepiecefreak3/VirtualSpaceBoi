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

        public event SysEventHandler SysCall;

        public abstract string Name { get; }

        public abstract object[] ServiceDispatch(params object[] args);

        protected void Print(string message) => Console.WriteLine($"{Name}: {message}");

        protected string GetHandle(string port) => (string)SysCall(this, 0, new object[] { port }).First();
        protected void FreeHandle(string handle) => SysCall(this, 2, new object[] { handle });
        protected object[] SendSyncRequest(string handle, params object[] args)
        {
            var args2 = new List<object>();
            args2.Add(handle);
            args2.AddRange(args);
            return SysCall(this, 1, args2.ToArray());
        }
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
