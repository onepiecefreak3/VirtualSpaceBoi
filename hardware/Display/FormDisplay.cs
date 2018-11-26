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
using System.Threading;
using System.Windows.Forms;
using Contract.Hardware;

namespace Display
{
    public partial class FormDisplay : Form, IDisplay
    {
        private byte[] _frameBuffer;

        public string MAC => "1A2B3C4D5E6F";

        public FormDisplay()
        {
            InitializeComponent();

            Name = "Display";
            FormClosed += FormDisplay_FormClosed;

            _frameBuffer = new byte[776 * 426 * 4];
            picDisplay.Image = new Bitmap(picDisplay.Width, picDisplay.Height);
            Show();
        }

        private void FormDisplay_FormClosed(object sender, FormClosedEventArgs e)
        {
            Kill(this);
        }

        public event KillEventHandler Kill;

        public unsafe void SetFrameBuffer(byte[] frame)
        {
            var data = (picDisplay.Image as Bitmap).LockBits(new Rectangle(0, 0, picDisplay.Image.Width, picDisplay.Image.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var point = (byte*)data.Scan0;

            for (int i = 0; i < _frameBuffer.Length; i++)
                point[i] = frame[i];

            (picDisplay.Image as Bitmap).UnlockBits(data);
        }
    }
}
