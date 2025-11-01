using System;
using System.Drawing;
using System.Windows.Forms;

namespace NetworkScreensaver
{
    public partial class MinimalScreensaver : Form
    {
        public MinimalScreensaver()
        {
            // Minimal setup
            BackColor = Color.Black;
            ForeColor = Color.Lime;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            TopMost = true;
            
            KeyPreview = true;
            KeyDown += (s, e) => Close();
            MouseMove += (s, e) => Close();
            MouseClick += (s, e) => Close();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            var text = "Minimal Screensaver Test - Press any key to exit";
            var font = SystemFonts.DefaultFont;
            var size = e.Graphics.MeasureString(text, font);
            var x = (Width - size.Width) / 2;
            var y = (Height - size.Height) / 2;
            
            e.Graphics.DrawString(text, font, Brushes.Lime, x, y);
        }
    }
}