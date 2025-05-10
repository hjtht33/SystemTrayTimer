using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SystemTrayTimer.Extensions;

namespace SystemTrayTimer
{
    public class AlertManager : IDisposable
    {
        // 窗体跟踪集合
        private readonly List<Form> _activeForms = new List<Form>();
        private ScreenBlanker _screenBlanker;
        private bool _disposed;

        // 事件定义
        public event Action<string> ErrorOccurred;
        public event Action<string> ShowPositionedAlertRequested;
        public AudioAlertService AudioService { get; } = new AudioAlertService();
        public AlertTextManager TextManager { get; } = new AlertTextManager();
        public bool MinimizeActiveWindow { get; set; }
        public bool EnableNotifications { get; set; }
        public bool BlankScreen { get; set; }
        public bool PositionAlertAtCursor { get; set; }


        public AlertManager()
        {
            // 绑定内部事件处理
            ShowPositionedAlertRequested += ShowPositionedAlert;
            // 绑定音频服务事件
            AudioService.AlertTriggered += message =>
            {
                // 这里可以统一处理音频服务的通知
                ErrorOccurred?.Invoke(message);

                // 如果需要直接显示弹窗可添加：
                // ShowPositionedAlert(message);
            };
        }
        public void ConfigureAudio(bool useSystemSound, string customSoundPath, bool enableFade)
        {
            AudioService.UseSystemSound = useSystemSound;
            AudioService.CustomSoundPath = customSoundPath;
            AudioService.EnableFade = enableFade;
        }

        public void Configure(bool minimizeActiveWindow, bool enableNotifications,
            bool blankScreen, bool positionAlertAtCursor)
        {
            MinimizeActiveWindow = minimizeActiveWindow;
            EnableNotifications = enableNotifications;
            BlankScreen = blankScreen;
            PositionAlertAtCursor = positionAlertAtCursor;

        }

        public void TriggerAlert()
        {
            try
            {
                ExecutePreAlertActions();
                ShowAlertIndicators();
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(ex.Message);
            }
        }

        private void ExecutePreAlertActions()
        {
            if (MinimizeActiveWindow)
            {
                MinimizeWindowAtCursor();
            }

            if (BlankScreen)
            {
                _screenBlanker = new ScreenBlanker();
                _screenBlanker.BlankScreens();
            }
        }

        private void ShowAlertIndicators()
        {

            // 获取当前提示文本
            string alertText = GetCurrentAlertText();

            if (EnableNotifications)
            {
                AudioService.PlayAlert();
            }

            if (PositionAlertAtCursor)
            {
                // 传递文本参数
                ShowPositionedAlertRequested?.Invoke(alertText);
            }
        }

        private string GetCurrentAlertText()
        {
            // 优先使用最新保存的自定义文本
            return TextManager.AlertTexts.LastOrDefault()
                ?? GetDefaultAlertText();
        }

        private string GetDefaultAlertText()
        {
            return "健康提醒：请暂停当前操作";
        }


        private void MinimizeWindowAtCursor()
        {
            NativeMethods.MinimizeTargetWindowAtCursor();
        }
        public void ClearAlertHistory()
        {
            TextManager.ClearAll();
        }

        private void ShowPositionedAlert(string alertText)
        {
            var popup = new CursorPositionAlertForm(alertText)
            {
                StartPosition = FormStartPosition.Manual,
                Location = Cursor.Position
            };

            RegisterForm(popup);
            popup.Show();
            Application.DoEvents(); // 确保立即显示
        }

        // 新增公共调用方法
        public void ShowCustomAlert(string customText = null)
        {
            string text = customText ?? GetCurrentAlertText();
            ShowPositionedAlert(text);
        }

        public void RegisterForm(Form form)
        {
            _activeForms.Add(form);
            form.FormClosed += (s, e) => _activeForms.Remove(form);
        }

        public void RestoreState()
        {
            _screenBlanker?.RestoreScreens();
            _screenBlanker?.Dispose();
        }

        private void LoadSettings()
        {
            // 加载音频设置
            ConfigureAudio(
                Properties.Settings.Default.UseSystemSound,
                Properties.Settings.Default.CustomSoundPath,
                Properties.Settings.Default.EnableFade
            );

            // 加载提醒设置
            Configure(
                Properties.Settings.Default.MinimizeActiveWindow,
                Properties.Settings.Default.EnableNotifications,
                Properties.Settings.Default.BlankScreenOnAlert,
                Properties.Settings.Default.PositionAlertAtCursor
            );
        }

        public void SaveSettings()
        {
            try
            {
                // 音频配置
                Properties.Settings.Default.UseSystemSound = AudioService.UseSystemSound;
                Properties.Settings.Default.CustomSoundPath = AudioService.CustomSoundPath;
                Properties.Settings.Default.EnableFade = AudioService.EnableFade;

                // 提醒配置
                Properties.Settings.Default.MinimizeActiveWindow = MinimizeActiveWindow;
                Properties.Settings.Default.EnableNotifications = EnableNotifications;
                Properties.Settings.Default.BlankScreenOnAlert = BlankScreen;
                Properties.Settings.Default.PositionAlertAtCursor = PositionAlertAtCursor;

                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"保存设置失败: {ex.Message}");
            }
        }

        // 实现IDisposable模式
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // 清理托管资源
                ShowPositionedAlertRequested -= ShowPositionedAlert;
                CloseAllForms();
                _screenBlanker?.Dispose();
            }

            // 清理非托管资源（如果有）
            // NativeMethods.Cleanup();

            _disposed = true;
        }
        private void CloseAllForms()
        {
            foreach (var form in _activeForms.ToArray())
            {
                form.SafeInvoke(f =>
                {
                    if (!f.IsDisposed)
                    {
                        f.Close();
                        f.Dispose();
                    }
                });
            }
            _activeForms.Clear();
        }
        public void TextFun()
        {

        }

    }

}
