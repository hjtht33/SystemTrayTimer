using System;
using System.Drawing;
using System.Windows.Forms;

namespace SystemTrayTimer
{
    internal class AlertOptionsForm : Form
    {
        private readonly AlertManager _alertManager;

        public AlertOptionsForm(AlertManager alertManager)
        {
            _alertManager = alertManager;
            InitializeComponents();
            LoadSettings();
        }

        //private void InitializeComponents()
        //{
        //    this.Text = "提醒选项设置";
        //    this.Size = new Size(400, 220); // 优化窗体尺寸
        //    this.FormBorderStyle = FormBorderStyle.FixedDialog;
        //    this.StartPosition = FormStartPosition.CenterScreen;

        //    // 主布局容器
        //    var mainTable = new TableLayoutPanel
        //    {
        //        Dock = DockStyle.Fill,
        //        ColumnCount = 1,
        //        RowCount = 5,
        //        Padding = new Padding(15),
        //        CellBorderStyle = TableLayoutPanelCellBorderStyle.None
        //    };

        //    // 最小化选项行 - 使用双列布局
        //    var minimizeRow = new TableLayoutPanel
        //    {
        //        Dock = DockStyle.Fill,
        //        ColumnCount = 2,
        //        RowCount = 1,
        //        Height = 35,
        //        Margin = new Padding(0, 0, 0, 10)
        //    };
        //    minimizeRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
        //    minimizeRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));

        //    var chkMinimize = new CheckBox
        //    {
        //        Text = "最小化当前活动窗口",
        //        AutoSize = true,
        //        Anchor = AnchorStyles.Left
        //    };

        //    var btnWhitelist = new Button
        //    {
        //        Text = "配置...",
        //        Size = new Size(75, 26),
        //        Anchor = AnchorStyles.Right
        //    };

        //    // 配置按钮点击事件
        //    btnWhitelist.Click += (s, e) => new ProcessSelectorForm().ShowDialog();

        //    minimizeRow.Controls.Add(chkMinimize, 0, 0);
        //    minimizeRow.Controls.Add(btnWhitelist, 1, 0);

        //    // 其他选项复选框
        //    var chkNotifications = CreateOptionCheckBox("启用系统通知");
        //    var chkBlankScreen = CreateOptionCheckBox("全屏黑屏提醒");
        //    var chkPositionAlert = CreateOptionCheckBox("在鼠标位置显示弹窗");

        //    // 按钮面板
        //    var buttonPanel = new FlowLayoutPanel
        //    {
        //        Dock = DockStyle.Bottom,
        //        FlowDirection = FlowDirection.RightToLeft,
        //        Height = 40,
        //        Padding = new Padding(0, 5, 15, 0)
        //    };

        //    var btnCancel = new Button
        //    {
        //        Text = "取消",
        //        Size = new Size(80, 28),
        //        DialogResult = DialogResult.Cancel
        //    };

        //    var btnSave = new Button
        //    {
        //        Text = "保存",
        //        Size = new Size(80, 28),
        //        DialogResult = DialogResult.OK
        //    };

        //    buttonPanel.Controls.AddRange(new[] { btnCancel, btnSave });

        //    // 构建主界面
        //    mainTable.Controls.Add(minimizeRow, 0, 0);
        //    mainTable.Controls.Add(chkNotifications, 0, 1);
        //    mainTable.Controls.Add(chkBlankScreen, 0, 2);
        //    mainTable.Controls.Add(chkPositionAlert, 0, 3);
        //    mainTable.Controls.Add(buttonPanel, 0, 4);

        //    this.Controls.Add(mainTable);

        //    // 数据绑定配置
        //    ConfigureDataBindings(chkMinimize, chkNotifications, chkBlankScreen, chkPositionAlert);
        //}
        private void InitializeComponents()
        {
            this.Text = "提醒选项设置";
            this.Size = new Size(520, 300);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;

            // 主布局容器 - 2列布局
            var mainTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(15),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                ColumnStyles =
            {
                new ColumnStyle(SizeType.Percent, 70F),
                new ColumnStyle(SizeType.Percent, 30F)
            }
            };

            // 选项配置方法
            void AddOptionRow(Control ctrl, int rowIndex, string buttonText = null, EventHandler clickHandler = null)
            {
                // 选项控件
                ctrl.Dock = DockStyle.Fill;
                mainTable.Controls.Add(ctrl, 0, rowIndex);

                // 配置按钮（可选）
                if (!string.IsNullOrEmpty(buttonText))
                {
                    var btn = new Button
                    {
                        Text = buttonText,
                        Size = new Size(90, 28),
                        Anchor = AnchorStyles.Right
                    };
                    if (clickHandler != null)
                        btn.Click += clickHandler;

                    mainTable.Controls.Add(btn, 1, rowIndex);
                }
            }

            // 最小化选项
            var chkMinimize = new CheckBox
            {
                Text = "最小化当前活动窗口",
                AutoSize = true
            };
            AddOptionRow(chkMinimize, 0, "配置...", (s, e) => new ProcessSelectorForm().ShowDialog());

            // 系统通知
            var chkNotifications = CreateOptionCheckBox("启用系统通知");
            AddOptionRow(chkNotifications, 1, "测试通知", (s, e) => _alertManager.AudioService.PlayAlert());

            // 黑屏提醒
            var chkBlankScreen = CreateOptionCheckBox("全屏黑屏提醒");
            AddOptionRow(chkBlankScreen, 2, "预览效果", (s, e) => _alertManager.TextFun());

            // 鼠标位置弹窗
            var chkPositionAlert = CreateOptionCheckBox("在鼠标位置显示弹窗");
            AddOptionRow(chkPositionAlert, 3, "编辑内容", (s, e) =>
                new CustomAlertPanel(_alertManager.TextManager).ShowDialog());

            // 按钮面板
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true
            };

            var btnCancel = new Button { Text = "取消", DialogResult = DialogResult.Cancel };
            var btnSave = new Button { Text = "保存", DialogResult = DialogResult.OK };
            buttonPanel.Controls.AddRange(new[] { btnCancel, btnSave });

            mainTable.Controls.Add(buttonPanel, 0, 4);
            mainTable.SetColumnSpan(buttonPanel, 2);

            this.Controls.Add(mainTable);

            // 数据绑定
            ConfigureDataBindings(chkMinimize, chkNotifications, chkBlankScreen, chkPositionAlert);
        }

        private CheckBox CreateOptionCheckBox(string text)
        {
            return new CheckBox
            {
                Text = text,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 10),
                Anchor = AnchorStyles.Left
            };
        }

        private void ConfigureDataBindings(params CheckBox[] checkboxes)
        {
            checkboxes[0].DataBindings.Add("Checked", _alertManager, "MinimizeActiveWindow",
                false, DataSourceUpdateMode.OnPropertyChanged);

            checkboxes[1].DataBindings.Add("Checked", _alertManager, "EnableNotifications",
                false, DataSourceUpdateMode.OnPropertyChanged);

            checkboxes[2].DataBindings.Add("Checked", _alertManager, "BlankScreen",
                false, DataSourceUpdateMode.OnPropertyChanged);

            checkboxes[3].DataBindings.Add("Checked", _alertManager, "PositionAlertAtCursor",
                false, DataSourceUpdateMode.OnPropertyChanged);
        }

        private void LoadSettings()
        {
            // 通过AlertManager自动加载设置
        }
    }
}

