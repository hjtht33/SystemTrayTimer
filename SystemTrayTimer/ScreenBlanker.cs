using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SystemTrayTimer
{
    internal class ScreenBlanker : IDisposable
    {
        private readonly List<Form> _blankForms = new List<Form>();
        private System.Windows.Forms.Timer _closeTimer;

        public void BlankScreens()
        {
            RestoreScreens(); // 清理现存实例

            foreach (Screen screen in Screen.AllScreens)
            {
                var blankForm = new Form
                {
                    FormBorderStyle = FormBorderStyle.None,
                    BackColor = Color.Black,
                    Bounds = screen.Bounds,
                    TopMost = true,
                    ShowInTaskbar = false,
                    Cursor = Cursors.No,
                    WindowState = FormWindowState.Maximized,
                    Enabled = false // 保持窗体自身禁用状态
                };

                // 添加提示文字
                var lblMessage = new Label
                {
                    Text = "现在是休息时间",
                    Font = new Font("微软雅黑", 24, FontStyle.Bold),
                    ForeColor = Color.White,
                    Dock = DockStyle.Fill,  // 使用Dock替代手动计算位置
                    TextAlign = ContentAlignment.MiddleCenter
                };

                blankForm.Controls.Add(lblMessage);
                blankForm.Show();
                _blankForms.Add(blankForm);
            }

            // 配置自动关闭计时器
            _closeTimer = new System.Windows.Forms.Timer { Interval = 5000 };
            _closeTimer.Tick += (s, e) => RestoreScreens();
            _closeTimer.Start();
        }

        public void RestoreScreens()
        {
            _closeTimer?.Stop();

            foreach (var form in _blankForms)
            {
                form.Close();
                form.Dispose();
            }
            _blankForms.Clear();
        }

        public void Dispose()
        {
            RestoreScreens();
            _closeTimer?.Dispose();
        }


    }
}

