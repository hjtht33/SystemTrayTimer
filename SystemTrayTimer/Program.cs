using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SystemTrayTimer
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        private static Mutex _mutex;
        [STAThread]
        static void Main()
        {
            const string appId = "YourUniqueAppIdentifier";
            _mutex = new Mutex(true, appId, out bool createdNew);

            if (!createdNew)
            {
               
                // 已存在实例，直接退出
                MessageBox.Show("程序已在运行中");
                return;

            }
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());

            GC.KeepAlive(_mutex); // 防止 Mutex 被回收
        }
    }
}
