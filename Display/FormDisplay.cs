using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Contract;

namespace Display
{
    [Export(typeof(IHardware))]
    [HardwareMeta(Name = "Display", MAC = "1A2B3C4D5E6F")]
    public partial class FormDisplay : Form, IDisplay
    {
        private byte[] _frameBuffer;

        public FormDisplay()
        {
            InitializeComponent();

            _frameBuffer = new byte[776 * 426 * 4];
            Show();
        }

        public void SetFrameBuffer(byte[] frame)
        {
            var test = new byte[0x100];
            for (int i = 0; i < 0x100; i++)
                test[i] = 0xFF;
            Array.Copy(frame, 0, _frameBuffer, 0, _frameBuffer.Length);
            Array.Copy(test, 0, _frameBuffer, 0, test.Length);
            picDisplay.Image = Bitmap.FromStream(new MemoryStream(_frameBuffer));
        }
    }
}
