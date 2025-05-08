using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SystemTrayTimer
{
    internal class CursorPositionAlertForm : Form
    {
        
        private Timer autoCloseTimer;
        private Timer followMouseTimer;
        private DateTime startTime;

        public CursorPositionAlertForm(string alertText)
        {
            InitializeComponent(alertText);
        }

        private void InitializeComponent(string alertText)
        {
            // 基础窗体设置
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.TopMost = true;
            this.MaximumSize = new Size(400, 0);  // 限制最大宽度
            this.AutoSize = true;                // 启用自动尺寸
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.Padding = new Padding(10);      // 添加内边距
            this.BackColor = Color.White;  // 更好的可见性

            // 文本标签设置
            var label = new Label
            {
                Text = alertText,
                AutoSize = true,
                MaximumSize = new Size(380, 0),  // 考虑内边后的有效宽度
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Black,
                Font = new Font("微软雅黑", 12, FontStyle.Bold),
                Dock = DockStyle.Fill
            };

            Controls.Add(label);

            // 自动关闭计时器
            autoCloseTimer = new Timer { Interval = 4000 };
            autoCloseTimer.Tick += (s, e) => Close();
            autoCloseTimer.Start();

            // 鼠标跟随计时器
            followMouseTimer = new Timer { Interval = 16 };
            followMouseTimer.Tick += FollowMouseHandler;
            followMouseTimer.Start();

            startTime = DateTime.Now;
        }

        private void FollowMouseHandler(object sender, EventArgs e)
        {
            // 更新窗体位置
            this.Location = Control.MousePosition;

            // 2秒后停止跟随
            if ((DateTime.Now - startTime).TotalSeconds >= 2)
            {
                followMouseTimer.Stop();
            }
        }

        // 添加圆角样式
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var path = GetRoundedPath(ClientRectangle, 5))
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Region = new Region(path);
                using (var pen = new Pen(Color.Black, 1))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }
        // 增加资源释放
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                autoCloseTimer?.Dispose();
                followMouseTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

