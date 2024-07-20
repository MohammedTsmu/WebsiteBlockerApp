using System;
using System.Drawing;
using System.Linq;
using System.Timers;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Microsoft.Win32;

namespace WebsiteBlockerApp
{
    public partial class MainForm : Form
    {
        private readonly Button btnAddDistractionSite;
        private readonly Button btnRemoveDistractionSite;
        private readonly Button btnToggleFocusMode;
        private readonly ListBox listPornSites;
        private readonly ListBox listDistractionSites;
        private readonly Label lblStatus;
        private readonly NotifyIcon trayIcon;
        private readonly ContextMenu trayMenu;
        private Blocker blocker;
        private readonly System.Timers.Timer statusUpdateTimer;

        public MainForm()
        {
            // تأكد من تهيئة blocker قبل استخدامه
            blocker = new Blocker();

            ShowSplashScreen(); // عرض شاشة البداية

            this.btnAddDistractionSite = new Button();
            this.btnRemoveDistractionSite = new Button();
            this.btnToggleFocusMode = new Button();
            this.listPornSites = new ListBox();
            this.listDistractionSites = new ListBox();
            this.lblStatus = new Label();
            this.trayMenu = new ContextMenu();
            this.trayIcon = new NotifyIcon();

            InitializeComponents();
            UpdateStatus();

            statusUpdateTimer = new System.Timers.Timer(1000); // تحديث كل ثانية
            statusUpdateTimer.Elapsed += (sender, e) => UpdateStatus();
            statusUpdateTimer.Start();

            AddToStartup(); // التأكد من إضافة التطبيق إلى بدء التشغيل عند تشغيل التطبيق
        }

        private async void ShowSplashScreen()
        {
            SplashScreenForm splashScreen = new SplashScreenForm();
            splashScreen.Show();
            await blocker.LoadSites(); // تحميل المواقع أثناء عرض شاشة البداية
            splashScreen.Close();
        }

        private void InitializeComponents()
        {
            // 
            // btnAddDistractionSite
            // 
            this.btnAddDistractionSite.Location = new System.Drawing.Point(12, 12);
            this.btnAddDistractionSite.Name = "btnAddDistractionSite";
            this.btnAddDistractionSite.Size = new System.Drawing.Size(150, 30);
            this.btnAddDistractionSite.Text = "Add Distraction Site";
            this.btnAddDistractionSite.BackColor = Color.FromArgb(33, 150, 243);
            this.btnAddDistractionSite.ForeColor = Color.White;
            this.btnAddDistractionSite.FlatStyle = FlatStyle.Flat;
            this.btnAddDistractionSite.MouseEnter += (s, e) => { btnAddDistractionSite.BackColor = Color.FromArgb(25, 118, 210); };
            this.btnAddDistractionSite.MouseLeave += (s, e) => { btnAddDistractionSite.BackColor = Color.FromArgb(33, 150, 243); };
            this.btnAddDistractionSite.Click += new EventHandler(this.BtnAddDistractionSite_Click);

            // 
            // btnRemoveDistractionSite
            // 
            this.btnRemoveDistractionSite.Location = new System.Drawing.Point(12, 48);
            this.btnRemoveDistractionSite.Name = "btnRemoveDistractionSite";
            this.btnRemoveDistractionSite.Size = new System.Drawing.Size(150, 30);
            this.btnRemoveDistractionSite.Text = "Remove Distraction Site";
            this.btnRemoveDistractionSite.BackColor = Color.FromArgb(244, 67, 54);
            this.btnRemoveDistractionSite.ForeColor = Color.White;
            this.btnRemoveDistractionSite.FlatStyle = FlatStyle.Flat;
            this.btnRemoveDistractionSite.MouseEnter += (s, e) => { btnRemoveDistractionSite.BackColor = Color.FromArgb(211, 47, 47); };
            this.btnRemoveDistractionSite.MouseLeave += (s, e) => { btnRemoveDistractionSite.BackColor = Color.FromArgb(244, 67, 54); };
            this.btnRemoveDistractionSite.Click += new EventHandler(this.BtnRemoveDistractionSite_Click);

            // 
            // btnToggleFocusMode
            // 
            this.btnToggleFocusMode.Location = new System.Drawing.Point(180, 12);
            this.btnToggleFocusMode.Name = "btnToggleFocusMode";
            this.btnToggleFocusMode.Size = new System.Drawing.Size(150, 66);
            this.btnToggleFocusMode.Text = "Disable Focus Mode";
            this.btnToggleFocusMode.BackColor = Color.FromArgb(76, 175, 80);
            this.btnToggleFocusMode.ForeColor = Color.White;
            this.btnToggleFocusMode.FlatStyle = FlatStyle.Flat;
            this.btnToggleFocusMode.MouseEnter += (s, e) => { btnToggleFocusMode.BackColor = Color.FromArgb(56, 142, 60); };
            this.btnToggleFocusMode.MouseLeave += (s, e) => { btnToggleFocusMode.BackColor = Color.FromArgb(76, 175, 80); };
            this.btnToggleFocusMode.Click += new EventHandler(this.BtnToggleFocusMode_Click);

            // 
            // listPornSites
            // 
            this.listPornSites.FormattingEnabled = true;
            this.listPornSites.Location = new System.Drawing.Point(12, 84);
            this.listPornSites.Name = "listPornSites";
            this.listPornSites.Size = new System.Drawing.Size(318, 95);
            this.listPornSites.BackColor = Color.White;

            // 
            // listDistractionSites
            // 
            this.listDistractionSites.FormattingEnabled = true;
            this.listDistractionSites.Location = new System.Drawing.Point(12, 185);
            this.listDistractionSites.Name = "listDistractionSites";
            this.listDistractionSites.Size = new System.Drawing.Size(318, 95);
            this.listDistractionSites.BackColor = Color.White;

            // 
            // lblStatus
            // 
            this.lblStatus.Location = new System.Drawing.Point(12, 290);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(318, 23);
            this.lblStatus.Text = "Status";
            this.lblStatus.ForeColor = Color.Black;

            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(342, 321);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.listDistractionSites);
            this.Controls.Add(this.listPornSites);
            this.Controls.Add(this.btnToggleFocusMode);
            this.Controls.Add(this.btnRemoveDistractionSite);
            this.Controls.Add(this.btnAddDistractionSite);
            this.Name = "MainForm";
            this.Text = "Website Blocker";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            trayMenu.MenuItems.Add("Exit", OnExit);
            trayMenu.MenuItems.Add("Restore", OnRestore);

            trayIcon.Text = "Website Blocker";
            trayIcon.Icon = new System.Drawing.Icon(SystemIcons.Application, 40, 40);
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;

            this.FormClosing += new FormClosingEventHandler(this.OnFormClosing);
            trayIcon.DoubleClick += new EventHandler(this.OnTrayIconDoubleClick);

            LoadBlockedSites();
        }

        private void BtnAddDistractionSite_Click(object sender, EventArgs e)
        {
            string site = Interaction.InputBox("Enter distraction site to block:", "Add Distraction Site", "example.com");
            if (!string.IsNullOrEmpty(site))
            {
                blocker.AddDistractionSite(site);
                listDistractionSites.Items.Add(site);
                UpdateStatus();
            }
        }

        private void BtnRemoveDistractionSite_Click(object sender, EventArgs e)
        {
            if (listDistractionSites.SelectedItem != null)
            {
                string site = listDistractionSites.SelectedItem.ToString();
                blocker.RemoveDistractionSite(site);
                listDistractionSites.Items.Remove(site);
                UpdateStatus();
            }
        }

        private void BtnToggleFocusMode_Click(object sender, EventArgs e)
        {
            if (blocker.GetDistractionSites().Any())
            {
                if (btnToggleFocusMode.Text == "Disable Focus Mode")
                {
                    blocker.DisableFocusMode();
                    blocker.RemoveDistractionSites(); // إزالة المواقع التي تشتت الانتباه عند إيقاف وضع التركيز
                    btnToggleFocusMode.Text = "Enable Focus Mode";
                }
                else
                {
                    blocker.EnableFocusMode();
                    btnToggleFocusMode.Text = "Disable Focus Mode";
                }
                UpdateStatus();
            }
        }

        private void UpdateStatus()
        {
            if (lblStatus.InvokeRequired)
            {
                lblStatus.Invoke(new Action(UpdateStatus));
            }
            else
            {
                lblStatus.Text = blocker.IsDownloading
                    ? "Downloading list... Please wait."
                    : $"Porn Sites: {blocker.PornSitesCount}, Distraction Sites: {blocker.DistractionSitesCount}, Last Updated: {blocker.LastUpdated}, Focus Mode: {(btnToggleFocusMode.Text == "Disable Focus Mode" ? "Enabled" : "Disabled")}";
            }
        }

        private void LoadBlockedSites()
        {
            var pornSites = blocker.GetPornSites();
            listPornSites.Items.AddRange(pornSites.ToArray());

            var distractionSites = blocker.GetDistractionSites();
            listDistractionSites.Items.AddRange(distractionSites.ToArray());
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
            blocker.RemoveDistractionSites(); // إزالة المواقع التي تشتت الانتباه عند إغلاق التطبيق
        }

        private void OnExit(object sender, EventArgs e)
        {
            trayIcon.Visible = false; // إخفاء أيقونة التطبيق من منطقة الإعلام
            blocker.RemoveDistractionSites(); // إزالة المواقع التي تشتت الانتباه عند إغلاق التطبيق
            RemoveFromStartup(); // إزالة التطبيق من بدء التشغيل عند الخروج
            Application.Exit();
        }

        private void OnRestore(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void OnTrayIconDoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void AddToStartup()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    key.SetValue("WebsiteBlockerApp", "\"" + Application.ExecutablePath + "\"");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding to startup: " + ex.Message);
            }
        }

        private void RemoveFromStartup()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    key.DeleteValue("WebsiteBlockerApp", false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error removing from startup: " + ex.Message);
            }
        }
    }
}
