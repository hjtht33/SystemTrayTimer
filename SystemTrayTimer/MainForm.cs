using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.IO;

namespace SystemTrayTimer
{
    internal class MainForm : Form
    {
       
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private FloatingWindow floatingWindow;
        private Timer countdownTimer;
        private int totalSeconds;
        private int remainingSeconds;
        private bool isLooping;
        private bool editMode;

        private PresetManager _presetManager;
        private AlertManager _alertManager;


        public MainForm()
        {
            var iconStream = Assembly.GetExecutingAssembly()
    .GetManifestResourceStream("SystemTrayTimer.Resources._texicon64.ico");
            var trayIcon = new Icon(iconStream);
            InitializeComponents();
            LoadSettings();
            this.Icon = trayIcon;
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            
        }
       

        private void InitializeComponents()
        {
            Console.WriteLine($"[Init] 进入核心初始化 - {DateTime.Now:HH:mm:ss.fff}");
            if (trayIcon != null)
            {
                Console.WriteLine($"[Init] 检测到托盘已存在，跳过");
                return;
            }
            Debug.WriteLine("开始初始化托盘系统...");
            // 核心组件存在性检查
            // 初始化核心组件
            InitializeAlertManager();
            InitializeTraySystem();
            InitializePresetManager();
            InitializeFloatingWindow();
            InitializeTimer();
            
        }
        
       


        private void InitializeTraySystem()
        {
            Console.WriteLine($"[{DateTime.Now}] 初始化托盘系统");
            if (trayIcon != null)
            {
                Console.WriteLine($"[{DateTime.Now}] 托盘图标已存在，跳过初始化");
                return;
            }

            // 创建托盘图标
            trayIcon = new NotifyIcon
            {
                Icon = new Icon("Resources/_texicon64.ico"),
                Text = "倒计时闹钟",
                Visible = true
            };

            // 构建统一菜单
            trayMenu = new ContextMenuStrip();

            // 倒计时功能菜单
            trayMenu.Items.Add("设置倒计时", null, SetCountdown);
            trayMenu.Items.Add(new ToolStripSeparator());

            // 循环计时菜单项
            var loopItem = new ToolStripMenuItem("循环计时")
            {
                CheckOnClick = true,
                Checked = false
            };
            loopItem.Click += ToggleLoop;
            trayMenu.Items.Add(loopItem);

            // 提醒设置功能菜单
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add("音频设置", null, (s, e) => ShowAudioSettings()); 
            trayMenu.Items.Add("提醒选项", null, (s, e) => ShowAlertOptions());

            // 通用功能菜单
            trayMenu.Items.Add(new ToolStripSeparator());
            var editItem = new ToolStripMenuItem("编辑位置")
            {
                CheckOnClick = true,
                Checked = false
            };
            editItem.Click += ToggleEditMode;
            trayMenu.Items.Add(editItem);
            trayMenu.Items.Add("退出", null, OnExit);

            // 绑定菜单到托盘
            trayIcon.ContextMenuStrip = trayMenu;

            // 左键点击事件
            trayIcon.MouseClick -= TrayIcon_LeftClick;
            trayIcon.MouseClick += TrayIcon_LeftClick;
        }
        private void TrayIcon_LeftClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _presetManager.ShowPresetMenu(Cursor.Position);
            }
        }



        private void InitializePresetManager()
        {
            if (_presetManager != null) return;
            _presetManager = new PresetManager();

            // 添加预设模板
            _presetManager.AddPreset(new PresetManager.PresetItem
            {
                Name = "25分钟番茄钟",
                Seconds = 25 * 60,
                Modifiers = 0x0002,  // Ctrl
                Key = Keys.D2
            });

            _presetManager.AddPreset(new PresetManager.PresetItem
            {
                Name = "5分钟休息",
                Seconds = 5 * 60,
                Modifiers = 0x0002,  // Ctrl
                Key = Keys.D5
            });

            _presetManager.AddPreset(new PresetManager.PresetItem
            {
                Name = "10分钟休息",
                Seconds = 10 * 60,
                Modifiers = 0x0002,  // Ctrl
                Key = Keys.D1
            });

            // 绑定事件
            _presetManager.PresetTriggered += StartCountdown;
            _presetManager.HotkeyError += OnHotkeyError;
        }

        private void InitializeFloatingWindow()
        {
            floatingWindow = new FloatingWindow();
            floatingWindow.Visible = Properties.Settings.Default.FloatingWindowVisible;
        }

        private void InitializeTimer()
        {
            countdownTimer = new Timer { Interval = 1000 };
            countdownTimer.Tick += Timer_Tick;
        }
        private void InitializeAlertManager()
        {
            _alertManager = new AlertManager();

            // 配置音频参数
            _alertManager.ConfigureAudio(
                useSystemSound: Properties.Settings.Default.UseSystemSound,
                customSoundPath: Properties.Settings.Default.CustomSoundPath,
                enableFade: Properties.Settings.Default.EnableFade
            );

            // 修改事件处理程序（接收string参数）
            _alertManager.ShowPositionedAlertRequested += (alertText) =>
            {
                var popup = new CursorPositionAlertForm(alertText)
                {
                    Location = Cursor.Position
                };
                _alertManager.RegisterForm(popup);
                popup.Show();
            };

            // 示例：手动触发自定义提示
            //btnTestAlert.Click += (s, e) =>
            //{
            //    _alertManager.ShowCustomAlert("测试用临时提示内容");
            //};

            // 加载初始配置
            LoadAlertSettings();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            remainingSeconds--;
            UpdateDisplay();

            if (remainingSeconds > 0) return;

            countdownTimer.Stop();
            ShowAlert();
            if (isLooping) StartCountdown(totalSeconds);
        }


        private void SetCountdown(object sender, EventArgs e)
        {
            using (var inputForm = new InputForm())
            {
                if (inputForm.ShowDialog() == DialogResult.OK)
                {
                    StartCountdown(inputForm.Minutes * 60);
                }
            }
        }

        private void StartCountdown(int seconds)
        {
            totalSeconds = seconds;
            remainingSeconds = seconds;
            countdownTimer.Start();
            floatingWindow.Show();
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            var ts = TimeSpan.FromSeconds(remainingSeconds);
            floatingWindow.SetTime($"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}");
            trayIcon.Text = $"剩余时间: {ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }

        private void ShowAlert()
        {
            _alertManager.TriggerAlert();
            
        }

        private void ToggleLoop(object sender, EventArgs e)
        {
            isLooping = !isLooping;
            ((ToolStripMenuItem)sender).Checked = isLooping;
        }

        private void ToggleEditMode(object sender, EventArgs e)
        {
            editMode = !editMode;
            floatingWindow.SetEditMode(editMode);
            ((ToolStripMenuItem)sender).Checked = editMode; // 同步勾选状态
        }

        //音频

        //private void ShowSettings()
        //{
        //    using (var settingsForm = new AlertSettingsForm(_alertManager.AudioService))
        //    {
        //        if (settingsForm.ShowDialog() == DialogResult.OK)
        //        {
        //            // 统一通过AlertManager保存
        //            _alertManager.SaveSettings();
        //        }
        //    }
        //}

        //protected override void OnFormClosing(FormClosingEventArgs e)
        //{
        //    // 保存悬浮窗位置
        //    SaveSettings();   
        //    base.OnFormClosing(e);
        //}
        //

        private void LoadSettings()
        {
            // 这里可以添加设置加载逻辑
            try
            {
                // 加载悬浮窗位置
                var savedLocation = Properties.Settings.Default.FloatingWindowLocation;
                
                // 验证位置有效性（防止屏幕外显示）
                if (IsValidScreenPosition(savedLocation))
                {
                    floatingWindow.Location = savedLocation;
                    Console.WriteLine($"[Load] 位置有效，已设置到: {savedLocation}");
                }
                else
                {
                    Console.WriteLine("[Load] 位置无效，使用默认位置");
                    floatingWindow.Location = new Point(100, 100); // 默认位置
                }

                // 加载悬浮窗可见状态
                floatingWindow.Visible = Properties.Settings.Default.FloatingWindowVisible;
               
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"加载设置失败: {ex.Message}");
                // 使用默认值
                floatingWindow.Location = new Point(100, 100);
                floatingWindow.Visible = false;
            }
        }
        private void LoadAlertSettings()
        {
            // 改为调用Configure方法统一加载
            _alertManager.Configure(
                Properties.Settings.Default.MinimizeActiveWindow,
                Properties.Settings.Default.EnableNotifications,
                Properties.Settings.Default.BlankScreenOnAlert,
                Properties.Settings.Default.PositionAlertAtCursor
            );
        }

        // 验证屏幕位置是否有效
        private bool IsValidScreenPosition(Point position)
        {
            if (position.X < 0 || position.Y < 0) return false;

            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.WorkingArea.Contains(position))
                    return true;
            }
            return false;
        }

        private void ShowAudioSettings()
        {
            using (var settingsForm = new AlertSettingsForm(_alertManager.AudioService))
            {
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    // 保存设置到 AlertManager
                    _alertManager.ConfigureAudio(
                        _alertManager.AudioService.UseSystemSound,
                        _alertManager.AudioService.CustomSoundPath,
                        _alertManager.AudioService.EnableFade
                    );

                    // 持久化存储
                    Properties.Settings.Default.CustomSoundPath = _alertManager.AudioService.CustomSoundPath;
                    Properties.Settings.Default.UseSystemSound = _alertManager.AudioService.UseSystemSound;
                    Properties.Settings.Default.EnableFade = _alertManager.AudioService.EnableFade;
                    Properties.Settings.Default.Save();
                }
            }
        }
        private void ShowAlertOptions()
        {
            using (var optionsForm = new AlertOptionsForm(_alertManager))
            {
                if (optionsForm.ShowDialog() == DialogResult.OK)
                {
                    // 直接调用AlertManager的保存
                    _alertManager.SaveSettings();
                }
            }
        }

        //private void SaveSettings()
        //{
        //    // 这里可以添加设置保存逻辑
        //    try
        //    {
        //        // 保存悬浮窗状态
        //        var location = floatingWindow.Location;
                
        //        Properties.Settings.Default.FloatingWindowLocation = floatingWindow.Location;
        //        Properties.Settings.Default.FloatingWindowVisible = floatingWindow.Visible;
        //        Properties.Settings.Default.Save();
                
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"保存设置失败: {ex.Message}");
        //    }
        //}

        private void OnExit(object sender, EventArgs e)
        {
            //SaveSettings();
            // 先隐藏托盘图标
            if (trayIcon != null)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
            }

            // 确保窗体资源释放
            this.Close();
            Application.Exit();
        }

        // 新增窗体Dispose逻辑
        protected override void Dispose(bool disposing)
        {
            Console.WriteLine($"[{DateTime.Now}] 开始释放资源...");
            if (disposing)
            {
                // 取消 PresetManager 事件订阅
                if (_presetManager != null)
                {
                    _presetManager.PresetTriggered -= StartCountdown;
                    _presetManager.HotkeyError -= OnHotkeyError; // 需要提取lambda为单独方法
                    _presetManager.Dispose();
                    _presetManager = null;
                }
                // 按创建逆序释放
                _alertManager?.Dispose();
                _presetManager?.Dispose();
                countdownTimer?.Dispose();
                floatingWindow?.Dispose();

                // 托盘图标
                if (trayIcon != null)
                {
                    trayIcon.Visible = false;
                    trayIcon.Dispose();
                    trayIcon = null;
                }

                // 菜单
                trayMenu?.Dispose();
            }
            // Native清理已在AlertManager处理
            base.Dispose(disposing);
            Console.WriteLine($"[{DateTime.Now}] 资源释放完成");
        }

        // 新增错误处理方法（替代lambda）
        private void OnHotkeyError(string msg)
        {
            MessageBox.Show(msg);
        }
        //一开始就缩小程序到系统托盘
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.Hide();
        }
        private void OnAlertTriggered(string message)
        {
            // 确保在主线程更新 UI
            if (InvokeRequired)
            {
                Invoke(new Action<string>(OnAlertTriggered), message);
                return;
            }
        }
    }
}