using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SystemTrayTimer
{
    public class PresetManager : NativeWindow
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public class PresetItem
        {
            public string Name { get; set; }
            public int Seconds { get; set; }
            public int Modifiers { get; set; }
            public Keys Key { get; set; }
        }

        public event Action<int> PresetTriggered;
        public event Action<string> HotkeyError;

        private readonly List<PresetItem> _presets = new List<PresetItem>();
        private readonly Dictionary<int, PresetItem> _registeredHotkeys = new Dictionary<int, PresetItem>();
        private int _hotkeyIdCounter = 0x0000;
        
        private bool _disposed;

        public PresetManager()
        {
            CreateHandle(new CreateParams());
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            const int WM_DESTROY = 0x0002;

            switch (m.Msg)
            {
                case WM_HOTKEY:
                    HandleHotkey(m.WParam.ToInt32());
                    break;
                case WM_DESTROY:
                    UnregisterAllHotkeys();
                    break;
            }

            base.WndProc(ref m);
        }

        private void HandleHotkey(int hotkeyId)
        {
            if (_registeredHotkeys.TryGetValue(hotkeyId, out var preset))
            {
                PresetTriggered?.Invoke(preset.Seconds);
            }
        }

        public void AddPreset(PresetItem preset)
        {
            _presets.Add(preset);
            RegisterPresetHotkey(preset);
        }

        private void RegisterPresetHotkey(PresetItem preset)
        {
            var id = ++_hotkeyIdCounter;
            if (RegisterHotKey(Handle, id, preset.Modifiers, (int)preset.Key))
            {
                _registeredHotkeys.Add(id, preset);
            }
            else
            {
                HotkeyError?.Invoke($"Failed to register: {GetHotkeyText(preset)}");
            }
        }

        private void UnregisterAllHotkeys()
        {
            foreach (var id in _registeredHotkeys.Keys.ToList())
            {
                UnregisterHotKey(Handle, id);
                _registeredHotkeys.Remove(id);
            }
        }

        public void ShowPresetMenu(Point location)
        {
            var menu = new ContextMenuStrip();
            

            // 添加菜单项
            foreach (var preset in _presets)
            {
                var item = new ToolStripMenuItem($"{preset.Name} ({GetHotkeyText(preset)})");
                item.Click += (s, e) => PresetTriggered?.Invoke(preset.Seconds);
                menu.Items.Add(item);
            }

            // 显示在鼠标位置
            menu.Show(location);
        }

        private string GetHotkeyText(PresetItem preset)
        {
            var modifiers = new List<string>();
            if ((preset.Modifiers & 0x0002) != 0) modifiers.Add("Ctrl");
            if ((preset.Modifiers & 0x0004) != 0) modifiers.Add("Shift");
            if ((preset.Modifiers & 0x0008) != 0) modifiers.Add("Alt");

            return modifiers.Any()
                ? $"{string.Join("+", modifiers)}+{preset.Key}"
                : preset.Key.ToString();
        }

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
                // 清理托管事件订阅
                PresetTriggered = null;
                HotkeyError = null;
            }

            UnregisterAllHotkeys();
            DestroyHandle();
            _disposed = true;
        }
    }
}
