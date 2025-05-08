using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SystemTrayTimer
{
    public class AlertTextManager : IDisposable
    {
        public const int MaxHistory = 5;
        public const int MaxInputLength = 20;
        private readonly string _storagePath;
        private bool _disposed;


        public IReadOnlyList<string> AlertTexts => _texts.AsReadOnly();
        private List<string> _texts = new List<string>();

        public AlertTextManager()
        {
            _storagePath = Path.Combine(Application.StartupPath, "alert_texts.txt");
            LoadHistory();
        }

        private void LoadHistory()
        {
            if (!File.Exists(_storagePath)) return;

            _texts = File.ReadAllLines(_storagePath)
                .Select(line => line.Length > MaxInputLength ? line.Substring(0, MaxInputLength) : line)
                .Reverse() // 新数据在前
                .Take(MaxHistory)
                .Reverse() // 恢复原始顺序
                .ToList();
        }

        public bool TryAddText(string text)
        {
            // 严格验证输入长度
            if (string.IsNullOrWhiteSpace(text) || text.Length > MaxInputLength)
                return false;
            // 去重检查
            if (_texts.Contains(text))
                return false;

            // 移除旧条目保证不超过MaxHistory
            if (_texts.Count >= MaxHistory)
            {
                _texts.RemoveAt(0);
            }

            _texts.Add(text);
            SaveToFile();
            return true;
        }

        public void RemoveText(string text)
        {
            if (_texts.Remove(text))
            {
                SaveToFile();
            }
        }

        public void ClearAll()
        {
            _texts.Clear();
            SaveToFile();
        }
       
        private void SaveToFile()
        {
            File.WriteAllLines(_storagePath, _texts);
        }
        // 新增清理方法（可选）
        public void Dispose()
        {
            if (_disposed) return;

            // 清理托管资源（当前无需操作，仅为未来扩展预留）
            _texts.Clear();
            _texts = null; // 非必须，但可加速GC回收

            _disposed = true;
        }
    }

}
