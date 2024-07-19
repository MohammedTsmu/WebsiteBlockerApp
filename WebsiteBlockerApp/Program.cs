using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebsiteBlockerApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // عرض شاشة البداية
            ShowSplashScreen();

            // بدء تشغيل النموذج الرئيسي بعد عرض شاشة البداية
            Application.Run(new MainForm());
        }

        private static void ShowSplashScreen()
        {
            SplashScreenForm splashScreen = new SplashScreenForm();
            splashScreen.Show();

            // تشغيل تحميل المواقع في الخلفية
            Blocker blocker = new Blocker();
            Task.Run(() => blocker.LoadSites()).Wait();

            splashScreen.Close();
        }
    }
}
