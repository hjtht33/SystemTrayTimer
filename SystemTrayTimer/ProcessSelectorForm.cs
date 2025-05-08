using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;


namespace SystemTrayTimer
{
    public partial class ProcessSelectorForm : Form
    {

        // 配置文件路径
        private static readonly string WhitelistPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "whitelist.config"
        );

        // 当前白名单数据
        private BindingList<string> whitelistData = new BindingList<string>();

        public ProcessSelectorForm()
        {
            if (!File.Exists(WhitelistPath))
            {
                File.WriteAllText(WhitelistPath, "");
            }
            //InitializeComponent();
            InitializeCustomComponents();
            LoadData();
        }

        //初始化UI组件
        private void InitializeCustomComponents()
        {
            // 数据表格
            dataGridView1 = new DataGridView { Dock = DockStyle.Top, Height = 300 };

            // 白名单列表
            listBoxWhitelist = new ListBox
            {
                Dock = DockStyle.Fill,
                DisplayMember = "ProcessName",

            };

            // 操作按钮
            var panel = new Panel { Dock = DockStyle.Bottom, Height = 40 };
            btnAdd = new Button { Text = "Add", Dock = DockStyle.Left, Width = 100 };
            btnRemove = new Button { Text = "Remove", Dock = DockStyle.Left, Width = 100 };

            // 布局
            panel.Controls.Add(btnRemove);
            panel.Controls.Add(btnAdd);

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,

                Panel1 = { Controls = { dataGridView1 } },
                Panel2 = { Controls = { listBoxWhitelist, panel } }
            };

            Controls.Add(split);

            // 事件绑定
            btnAdd.Click += BtnAdd_Click;
            btnRemove.Click += BtnRemove_Click;
            Load += (s, e) => LoadRunningProcesses();
            FormClosing += (s, e) => SaveWhitelist();
        }
        



        private void LoadData()
        {
            // 加载白名单
            if (File.Exists(WhitelistPath))
            {
                whitelistData = new BindingList<string>(
                    File.ReadAllLines(WhitelistPath)
                       .Where(line => !string.IsNullOrWhiteSpace(line))
                       .Distinct()
                       .ToList()
                );
            }

            listBoxWhitelist.DataSource = whitelistData;
        }

        

        // 加载运行中的进程
        private void LoadRunningProcesses()
        {
            var processes = Process.GetProcesses()
            .Where(p => !string.IsNullOrEmpty(p.MainWindowTitle))
            .Where(p => !IsSystemProcess(p.ProcessName))
            .Select(p => new ProcessInfo
            {
                ProcessName = p.ProcessName,
                WindowTitle = ShortenTitle(p.MainWindowTitle, 30), // 限制标题长度
                FilePath = GetProcessPath(p),
                IsInWhitelist = whitelistData.Contains(p.ProcessName)
            })
            .Distinct()
            .ToList();

            dataGridView1.DataSource = processes;
        }

        // 标题缩短方法
        private string ShortenTitle(string title, int maxLength)
        {
            return title.Length > maxLength ?
                title.Substring(0, maxLength) + "..." :
                title;
        }

        // 新增系统进程判断
        private bool IsSystemProcess(string processName)
        {
            var systemProcesses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "system", "system idle process", "svchost",
        "csrss", "wininit", "smss", "dwm"
    };

            return systemProcesses.Contains(processName);
        }

        // 获取进程路径（带错误处理）
        private string GetProcessPath(Process process)
        {
            try
            {
                // 使用更安全的路径获取方式
                return process.MainModule?.FileName ?? "未知路径";
            }
            catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 5)
            {
                return "权限不足";
            }
            catch (Exception)
            {
                return "路径不可用";
            }
        }
        
        // 保存白名单
        private void SaveWhitelist()
        {
            try
            {
                File.WriteAllLines(WhitelistPath, whitelistData.Distinct());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败: {ex.Message}");
            }
        }

        // 添加到白名单
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow?.DataBoundItem is ProcessInfo info)
            {
                if (!whitelistData.Contains(info.ProcessName))
                {
                    whitelistData.Add(info.ProcessName);
                    LoadRunningProcesses(); // 刷新列表状态
                }
            }
        }

        // 移除白名单
        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (listBoxWhitelist.SelectedItem is string selectedName)
            {
                whitelistData.Remove(selectedName);
                LoadRunningProcesses(); // 刷新列表状态
            }
        }

        // UI控件
        private DataGridView dataGridView1;
        private ListBox listBoxWhitelist;
        private Button btnAdd;
        private Button btnRemove;

        // 进程信息数据类
        private class ProcessInfo
        {
            public string ProcessName { get; set; }
            public string WindowTitle { get; set; }
            public string FilePath { get; set; }
            public bool IsInWhitelist { get; set; }
        }
    }
}
