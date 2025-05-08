using System;
using System.Drawing;
using System.Windows.Forms;

namespace SystemTrayTimer
{
    public class TrayIconManager : IDisposable
    {
        public event EventHandler ExitRequested;
        public event EventHandler EditPositionRequested;
        public event EventHandler ShowSettingsRequested;
        public event EventHandler ShowAudioSettingsRequested;
        public event EventHandler ShowPresetMenuRequested;
        public event EventHandler<bool> LoopToggled;

        private readonly NotifyIcon _trayIcon;
        private readonly ContextMenuStrip _trayMenu;
        private ToolStripMenuItem _loopMenuItem;

        public Icon Icon
        {
            get => _trayIcon.Icon;
            set => _trayIcon.Icon = value;
        }

        public string ToolTipText
        {
            get => _trayIcon.Text;
            set => _trayIcon.Text = value;
        }

        public TrayIconManager(Icon defaultIcon)
        {
            _trayIcon = new NotifyIcon
            {
                Icon = defaultIcon,
                Text = "倒计时闹钟",
                Visible = true
            };

            _trayMenu = new ContextMenuStrip();
            InitializeMenu();

            _trayIcon.ContextMenuStrip = _trayMenu;
            _trayIcon.MouseClick += TrayIcon_MouseClick;
        }

        private void InitializeMenu()
        {
            // 倒计时相关
            _trayMenu.Items.Add("设置倒计时", null, (s, e) => ShowSettingsRequested?.Invoke(this, EventArgs.Empty));
            _trayMenu.Items.Add(new ToolStripSeparator());

            // 循环菜单项
            _loopMenuItem = new ToolStripMenuItem("循环计时")
            {
                CheckOnClick = true
            };
            _loopMenuItem.Click += (s, e) => LoopToggled?.Invoke(this, _loopMenuItem.Checked);
            _trayMenu.Items.Add(_loopMenuItem);

            // 设置相关
            _trayMenu.Items.Add(new ToolStripSeparator());
            _trayMenu.Items.Add("音频设置", null, (s, e) => ShowAudioSettingsRequested?.Invoke(this, EventArgs.Empty));
            _trayMenu.Items.Add("提醒选项", null, (s, e) => ShowSettingsRequested?.Invoke(this, EventArgs.Empty));

            // 系统功能
            _trayMenu.Items.Add(new ToolStripSeparator());
            _trayMenu.Items.Add("编辑位置", null, (s, e) => EditPositionRequested?.Invoke(this, EventArgs.Empty));
            _trayMenu.Items.Add("退出", null, (s, e) => ExitRequested?.Invoke(this, EventArgs.Empty));
        }

        private void TrayIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ShowPresetMenuRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        public void ShowBalloonTip(string title, string message, ToolTipIcon icon = ToolTipIcon.Info)
        {
            _trayIcon.ShowBalloonTip(3000, title, message, icon);
        }

        public void UpdateLoopState(bool isLooping)
        {
            _loopMenuItem.Checked = isLooping;
        }

        public void Dispose()
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            _trayMenu.Dispose();
        }
    }
}
