using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace WebsiteBlockerApp
{
    public partial class SplashScreenForm : Form
    {
        private Image animatedImage;
        private bool currentlyAnimating = false;

        public SplashScreenForm()
        {
            InitializeComponent();
            InitializeAnimatedImage();
            this.BackColor = Color.Gray; // تغيير لون الخلفية إلى اللون الرمادي
            this.FormBorderStyle = FormBorderStyle.None; // إزالة إطار النموذج
        }

        private void InitializeAnimatedImage()
        {
            this.animatedImage = Properties.Resources.loading5; // تحميل الصورة من الموارد
            this.pictureBox.Image = animatedImage;
            this.StartAnimating();
        }

        private void StartAnimating()
        {
            if (!currentlyAnimating)
            {
                ImageAnimator.Animate(animatedImage, new EventHandler(this.OnFrameChanged));
                currentlyAnimating = true;
            }
        }

        private void OnFrameChanged(object sender, EventArgs e)
        {
            this.pictureBox.Invalidate();
        }

        private void PictureBox_Paint(object sender, PaintEventArgs e)
        {
            ImageAnimator.UpdateFrames();
            e.Graphics.Clear(this.pictureBox.BackColor); // مسح الخلفية لتجنب التكرار

            // حساب الإحداثيات لتوسيط الصورة وجعلها أصغر قليلاً
            int width = animatedImage.Width / 2;
            int height = animatedImage.Height / 2;
            int x = (this.pictureBox.Width - width) / 2;
            int y = (this.pictureBox.Height - height) / 2;

            e.Graphics.DrawImage(animatedImage, new Rectangle(x, y, width, height));
        }
    }
}
