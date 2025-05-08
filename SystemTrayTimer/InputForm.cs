using System.Drawing;
using System.Windows.Forms;

namespace SystemTrayTimer
{
    public class InputForm:Form
    {
        private NumericUpDown numericInput;
        private Button btnOk;

        public int Minutes => (int)numericInput.Value;

        public InputForm()
        {
            InitializeComponents();
            this.Text = "设置倒计时";
            this.Size = new Size(200, 140);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void InitializeComponents()
        {
            numericInput = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 120,
                Value = 1,
                Location = new Point(50, 20),
                Width = 100
            };

            btnOk = new Button
            {
                Text = "确定",
                DialogResult = DialogResult.OK,
                Location = new Point(60, 60)
            };

            this.Controls.Add(numericInput);
            this.Controls.Add(btnOk);
        }
    }
}

