using System;
using System.Windows.Forms;

namespace OrionGDWidget
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

            if (args.Length >= 2 && args[0] == "--preview")
            {
                using var form = new OrionGDForm();
                using var bmp = form.RenderToBitmap();
                bmp.Save(args[1], System.Drawing.Imaging.ImageFormat.Png);
                return;
            }

            Application.Run(new OrionGDForm());
        }
    }
}
