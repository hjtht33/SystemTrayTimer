using System;
using System.Drawing;
using System.Windows.Forms;

namespace SystemTrayTimer
{
    public class AlertSettingsForm : Form
    {
        private readonly AudioAlertService _alertService;

        public AlertSettingsForm(AudioAlertService alertService)
        {
            _alertService = alertService ?? throw new ArgumentNullException(nameof(alertService));
            InitializeComponents();
            LoadSettings();
        }

        private void InitializeComponents()
        {
            // 基础布局
            this.Text = "提醒设置";
            this.Size = new Size(430, 280);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;

            // 控件初始化
            var lblSoundPath = new Label { Text = "自定义声音:", Location = new Point(20, 20) };
            var txtSoundPath = new TextBox { Width = 200, Location = new Point(120, 20), ReadOnly = true };
            var btnBrowse = new Button { Text = "浏览...", Location = new Point(330, 20) };

            var chkSystemSound = new CheckBox { Text = "使用系统提示音", Location = new Point(20, 60) };
            var chkFadeEffect = new CheckBox { Text = "启用淡入淡出效果", Location = new Point(20, 90) };

            var btnTest = new Button { Text = "测试播放", Location = new Point(20, 160) };
            var btnSave = new Button { Text = "保存", Location = new Point(240, 200) };
            var btnCancel = new Button { Text = "保存并退出", Location = new Point(330, 200) };

            // 新增时长设置控件
            var lblDuration = new Label { Text = "最大时长(秒):", Location = new Point(20, 120) };
            var numDuration = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 3600,
                Value = 10,
                Location = new Point(120, 120),
                Width = 80
            };

            // 互斥逻辑：系统提示音和自定义路径二选一
            chkSystemSound.CheckedChanged += (s, e) =>
            {
                if (chkSystemSound.Checked) txtSoundPath.Text = string.Empty;
            };
            txtSoundPath.TextChanged += (s, e) =>
            {
                if (!string.IsNullOrEmpty(txtSoundPath.Text)) chkSystemSound.Checked = false;
            };

            // 事件绑定
            btnBrowse.Click += (s, e) => BrowseSoundFile(txtSoundPath);
            btnTest.Click += (s, e) => _alertService.PlayAlert();
            btnSave.Click += (s, e) => SaveSettings();
            btnCancel.Click += (s, e) => { SaveSettings(); this.Close();  };
            

            // 添加控件
            this.Controls.AddRange(new Control[] {
            lblSoundPath, txtSoundPath, btnBrowse,
            chkSystemSound, chkFadeEffect,
            lblDuration, numDuration,
            btnTest, btnSave, btnCancel
        });
        }

        private void LoadSettings()
        {
            var txtSoundPath = (TextBox)this.Controls[1];
            var chkSystemSound = (CheckBox)this.Controls[3];
            var chkFadeEffect = (CheckBox)this.Controls[4];
            var numDuration = (NumericUpDown)this.Controls[6];

            txtSoundPath.Text = _alertService.CustomSoundPath;
            chkSystemSound.Checked = _alertService.UseSystemSound;
            chkFadeEffect.Checked = _alertService.EnableFade;
            numDuration.Value = _alertService.MaxDuration / 1000;
        }

        private void BrowseSoundFile(TextBox targetTextBox)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "音频文件|*.wav;*.mp3";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    targetTextBox.Text = dialog.FileName;
                }
            }
        }

        private void SaveSettings()
        {
            var txtSoundPath = (TextBox)this.Controls[1];
            var chkSystemSound = (CheckBox)this.Controls[3];
            var chkFadeEffect = (CheckBox)this.Controls[4];
            var numDuration = (NumericUpDown)this.Controls[6];

            // 关键保存逻辑：优先使用自定义音频
            if (!string.IsNullOrEmpty(txtSoundPath.Text))
            {
                _alertService.UseSystemSound = false;
                _alertService.CustomSoundPath = txtSoundPath.Text;
            }
            else
            {
                _alertService.UseSystemSound = chkSystemSound.Checked;
            }
            _alertService.EnableFade = chkFadeEffect.Checked;
            _alertService.MaxDuration = (int)numDuration.Value * 1000;

            // 保存到应用设置
            Properties.Settings.Default.CustomSoundPath = txtSoundPath.Text;
            Properties.Settings.Default.UseSystemSound = chkSystemSound.Checked;
            Properties.Settings.Default.EnableFade = chkFadeEffect.Checked;
            Properties.Settings.Default.MaxDuration = _alertService.MaxDuration;
            Properties.Settings.Default.Save();

            


        }
    }
}
    
