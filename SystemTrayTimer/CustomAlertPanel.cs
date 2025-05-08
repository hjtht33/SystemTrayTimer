using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SystemTrayTimer.Properties;

namespace SystemTrayTimer
{
    internal class CustomAlertPanel:Form
    {
        private readonly AlertTextManager _manager;
        private ListBox _historyList;
        private TextBox _selectedTextDisplay;

        public string SelectedAlertText => _historyList.SelectedItem?.ToString() ?? "";

        public CustomAlertPanel(AlertTextManager manager)
        {
            _manager = manager;
            InitializeComponents();
            SetupContextMenu();
            SetupSelectionHandler(); // 新增选择事件处理
        }

        private void InitializeComponents()
        {
            this.Text = "文本设置";
            this.Size = new Size(340, 380);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.CenterParent;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1,
                Padding = new Padding(10)
            };

            // 文本输入框
            var txtInput = new TextBox
            {
                
                Multiline = true,
                Height = 100,
                Width = 300,
                ScrollBars = ScrollBars.Vertical,
                MaxLength = AlertTextManager.MaxInputLength
            };
            
            // 新增输入提示标签
            var lblInputCounter = new Label
            {
                Text = $"{AlertTextManager.MaxInputLength}/{AlertTextManager.MaxInputLength}",
                ForeColor = Color.Gray,
                AutoSize = true,
                Anchor = AnchorStyles.Right
            };
            // 输入长度提示容器
            var counterContainer = new Panel
            {
                Height = 20,
                Dock = DockStyle.Bottom,
                Padding = new Padding(0, 2, 5, 0)
            };
            counterContainer.Controls.Add(lblInputCounter);
            // 输入框容器（添加底部提示）
            var inputContainer = new Panel
            {
                Height = 100,
                Dock = DockStyle.Top
            };
            inputContainer.Controls.Add(txtInput);
            inputContainer.Controls.Add(counterContainer);

            // 实时输入监控
            txtInput.TextChanged += (s, e) =>
            {
                // 更新剩余字符提示
                var remaining = AlertTextManager.MaxInputLength - txtInput.Text.Length;
                lblInputCounter.Text = $"{remaining}/{AlertTextManager.MaxInputLength}";
                lblInputCounter.ForeColor = remaining < 3 ? Color.OrangeRed : Color.Gray;

                // 自动截断粘贴内容（防御性处理）
                if (txtInput.Text.Length > AlertTextManager.MaxInputLength)
                {
                    var cursorPos = txtInput.SelectionStart;
                    txtInput.Text = txtInput.Text.Substring(0, AlertTextManager.MaxInputLength);
                    txtInput.SelectionStart = cursorPos > txtInput.Text.Length ?
                        txtInput.Text.Length :
                        cursorPos;
                }
            };

            // 输入时显示提示（当接近限制时）
            txtInput.KeyPress += (s, e) =>
            {
                if (txtInput.Text.Length >= AlertTextManager.MaxInputLength &&
                    !char.IsControl(e.KeyChar))
                {
                    e.Handled = true;
                    SystemSounds.Beep.Play();
                    ShowToolTip("已达到最大输入长度限制！", txtInput);
                }
            };
            // 保存按钮
            var btnSave = new Button
            {
                Text = "保存自定义提醒",
                Height = 30
            };
            btnSave.Click += (s, e) =>
            {
                var inputText = txtInput.Text.Trim();
                if (string.IsNullOrEmpty(inputText)) return;

                if (!_manager.TryAddText(inputText))
                {
                    HandleAddFailure();
                    return;
                }

                UpdateHistoryList();
                txtInput.Clear();
            };
            // 新增选中文本显示区域
            var lblShow = new Label
            {
                Text = "显示文本",
                AutoSize = true
            };
            _selectedTextDisplay = new TextBox
            {
                Multiline = true,
                Height = 30,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Top,
                BackColor = SystemColors.Info
            };

            // 历史记录标签
            var lblHistory = new Label
            {
                Text = "最近保存的记录(鼠标点击删除)",
                AutoSize = true
            };

            // 历史记录列表
            _historyList = new ListBox
            {
                Height = 80,
                Width = 300,
                IntegralHeight = false
            };
            
            UpdateHistoryList();

            mainLayout.Controls.Add(inputContainer);
            mainLayout.Controls.Add(btnSave);
            mainLayout.Controls.Add(lblShow);
            mainLayout.Controls.Add(_selectedTextDisplay, 0, 3);
            mainLayout.Controls.Add(lblHistory);
            mainLayout.Controls.Add(_historyList);
            

            this.Controls.Add(mainLayout);
        }
        // 辅助方法：显示临时提示
        private void ShowToolTip(string message, Control target)
        {
            var toolTip = new ToolTip
            {
                ToolTipIcon = ToolTipIcon.Warning,
                IsBalloon = true,
                ToolTipTitle = "输入限制"
            };

            toolTip.Show(message, target, 1000);
        }
        private void HandleAddFailure()
        {
            var result = MessageBox.Show(
                "已达到最大保存数量，是否要清空旧记录？",
                "存储已满",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                _manager.ClearAll();
                UpdateHistoryList();
            }
        }
        private void SetupSelectionHandler()
        {
            // 列表选择变化事件
            _historyList.SelectedIndexChanged += (s, e) =>
            {
                UpdateSelectedDisplay();
            };

            // 双击应用选择
            _historyList.DoubleClick += (s, e) =>
            {
                if (SelectedAlertText.Length > 0)
                {
                    MessageBox.Show($"已选择文本：{SelectedAlertText}",
                        "选择确认",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            };
        }
        private void UpdateSelectedDisplay()
        {
            // 更新显示区域内容
            _selectedTextDisplay.Text = SelectedAlertText;

            // 高亮显示（可选）
            if (!string.IsNullOrEmpty(SelectedAlertText))
            {
                _selectedTextDisplay.BackColor = Color.LightCyan;
            }
            else
            {
                _selectedTextDisplay.BackColor = SystemColors.Info;
            }
        }
        private void SetupContextMenu()
        {
            var menu = new ContextMenuStrip();

            // 删除菜单项
            var deleteItem = new ToolStripMenuItem("删除");
            deleteItem.Click += (s, e) => DeleteSelectedItem();

            menu.Items.Add(deleteItem);
            _historyList.ContextMenuStrip = menu;

            // 动态启用状态
            menu.Opening += (s, e) =>
            {
                deleteItem.Enabled = _historyList.SelectedItems.Count > 0;
            };
        }
        private void DeleteSelectedItem()
        {
            if (_historyList.SelectedItem == null) return;

            if (MessageBox.Show(
                "确定要删除选中的记录吗？",
                "确认删除",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            ) != DialogResult.Yes) return;

            _manager.RemoveText(_historyList.SelectedItem.ToString());
            UpdateHistoryList();
        }

        private void UpdateHistoryList()
        {
            _historyList.BeginUpdate();
            _historyList.DataSource = null;
            _historyList.DataSource = _manager.AlertTexts;
            _historyList.EndUpdate();
        }
        // 支持键盘操作
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && _historyList.Focused)
            {
                DeleteSelectedItem();
            }
            base.OnKeyDown(e);
        }

    }
}
