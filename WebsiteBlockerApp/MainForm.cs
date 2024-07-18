using System;
using System.Drawing;
using System.Linq;
using System.Timers;
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
        private readonly Blocker blocker;
        private readonly System.Timers.Timer statusUpdateTimer;

        public MainForm()
        {
            this.btnAddDistractionSite = new Button();
            this.btnRemoveDistractionSite = new Button();
            this.btnToggleFocusMode = new Button();
            this.listPornSites = new ListBox();
            this.listDistractionSites = new ListBox();
            this.lblStatus = new Label();
            this.trayMenu = new ContextMenu();
            this.trayIcon = new NotifyIcon();
            this.blocker = new Blocker();

            InitializeComponents();
            UpdateStatus();

            statusUpdateTimer = new System.Timers.Timer(1000); // تحديث كل ثانية
            statusUpdateTimer.Elapsed += (sender, e) => UpdateStatus();
            statusUpdateTimer.Start();

            AddToStartup(); // التأكد من إضافة التطبيق إلى بدء التشغيل عند تشغيل التطبيق
        }

        private void InitializeComponents()
        {
            // 
            // btnAddDistractionSite
            // 
            this.btnAddDistractionSite.Location = new System.Drawing.Point(12, 12);
            this.btnAddDistractionSite.Name = "btnAddDistractionSite";
            this.btnAddDistractionSite.Size = new System.Drawing.Size(150, 23);
            this.btnAddDistractionSite.TabIndex = 0;
            this.btnAddDistractionSite.Text = "Add Distraction Site";
            this.btnAddDistractionSite.UseVisualStyleBackColor = true;
            this.btnAddDistractionSite.Click += new EventHandler(this.BtnAddDistractionSite_Click);

            // 
            // btnRemoveDistractionSite
            // 
            this.btnRemoveDistractionSite.Location = new System.Drawing.Point(12, 41);
            this.btnRemoveDistractionSite.Name = "btnRemoveDistractionSite";
            this.btnRemoveDistractionSite.Size = new System.Drawing.Size(150, 23);
            this.btnRemoveDistractionSite.TabIndex = 1;
            this.btnRemoveDistractionSite.Text = "Remove Distraction Site";
            this.btnRemoveDistractionSite.UseVisualStyleBackColor = true;
            this.btnRemoveDistractionSite.Click += new EventHandler(this.BtnRemoveDistractionSite_Click);

            // 
            // btnToggleFocusMode
            // 
            this.btnToggleFocusMode.Location = new System.Drawing.Point(168, 12);
            this.btnToggleFocusMode.Name = "btnToggleFocusMode";
            this.btnToggleFocusMode.Size = new System.Drawing.Size(150, 52);
            this.btnToggleFocusMode.TabIndex = 2;
            this.btnToggleFocusMode.Text = "Disable Focus Mode";
            this.btnToggleFocusMode.UseVisualStyleBackColor = true;
            this.btnToggleFocusMode.Click += new EventHandler(this.BtnToggleFocusMode_Click);

            // 
            // listPornSites
            // 
            this.listPornSites.FormattingEnabled = true;
            this.listPornSites.Location = new System.Drawing.Point(12, 70);
            this.listPornSites.Name = "listPornSites";
            this.listPornSites.Size = new System.Drawing.Size(260, 173);
            this.listPornSites.TabIndex = 3;

            // 
            // listDistractionSites
            // 
            this.listDistractionSites.FormattingEnabled = true;
            this.listDistractionSites.Location = new System.Drawing.Point(278, 70);
            this.listDistractionSites.Name = "listDistractionSites";
            this.listDistractionSites.Size = new System.Drawing.Size(260, 173);
            this.listDistractionSites.TabIndex = 4;

            // 
            // lblStatus
            // 
            this.lblStatus.Location = new System.Drawing.Point(12, 250);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(526, 23);
            this.lblStatus.TabIndex = 5;
            this.lblStatus.Text = "Status";

            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(550, 281);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.listDistractionSites);
            this.Controls.Add(this.listPornSites);
            this.Controls.Add(this.btnToggleFocusMode);
            this.Controls.Add(this.btnRemoveDistractionSite);
            this.Controls.Add(this.btnAddDistractionSite);
            this.Name = "MainForm";
            this.Text = "Website Blocker";

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
