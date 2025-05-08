using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SystemTrayTimer
{
    internal class NativeMethods
    {
        // 窗口操作常量
        public const int SW_MINIMIZE = 6;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOZORDER = 0x0004;
        public const uint SWP_FRAMECHANGED = 0x0020;

        // 必需添加的 API 声明
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            uint uFlags
        );

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(Point pt);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        // 精确进程白名单列表（需与实际进程名完全一致）
        private static IEnumerable<string> LoadWhitelist()
        {
            var path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "whitelist.config"
            );

            try
            {
                return File.Exists(path) ?
                    File.ReadAllLines(path)
                        .Where(line => !string.IsNullOrWhiteSpace(line))
                        .Select(line => line.Trim())  // 清除前后空格
                        .Distinct(StringComparer.OrdinalIgnoreCase) :
                    Enumerable.Empty<string>();
            }
            catch
            {
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// 最小化目标应用程序的主窗口
        /// </summary>
        public static void MinimizeTargetWindowAtCursor()
        {
            try
            {
                // 从文件加载最新白名单（每次实时读取保证数据最新）
                var whitelist = new HashSet<string>(
                    LoadWhitelist(),
                    StringComparer.OrdinalIgnoreCase  // 不区分大小写比较
                );

                // 获取光标下的窗口句柄
                var cursorPos = Cursor.Position;
                IntPtr hwnd = WindowFromPoint(cursorPos);
                if (hwnd == IntPtr.Zero) return;

                // 获取窗口所属进程ID
                GetWindowThreadProcessId(hwnd, out uint processId);
                if (processId == 0) return;

                // 获取进程对象
                using (Process process = Process.GetProcessById((int)processId))
                {
                    // 检查进程是否在白名单中（不区分大小写）
                    if (!whitelist.Contains(process.ProcessName))
                        return;

                    // 获取进程的主窗口句柄
                    IntPtr mainWindow = process.MainWindowHandle;
                    if (mainWindow == IntPtr.Zero || !IsWindowVisible(mainWindow))
                        return;

                    // 最小化主窗口并刷新窗口状态
                    ShowWindow(mainWindow, SW_MINIMIZE);
                    RefreshWindow(mainWindow);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"最小化失败: {ex.Message}");
            }
        }
        // 新增窗口刷新方法
        private static void RefreshWindow(IntPtr hWnd)
        {
            SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
        }

    }
}

