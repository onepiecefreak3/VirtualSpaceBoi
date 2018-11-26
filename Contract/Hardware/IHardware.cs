using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Hardware
{
    public interface IHardware
    {
        string Name { get; }
        string MAC { get; }

        event KillEventHandler Kill;
    }

    public delegate void KillEventHandler(IHardware sender);
}
