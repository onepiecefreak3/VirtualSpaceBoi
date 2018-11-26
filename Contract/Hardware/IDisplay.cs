using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Hardware
{
    public interface IDisplay : IHardware
    {
        void SetFrameBuffer(byte[] frame);
    }
}
