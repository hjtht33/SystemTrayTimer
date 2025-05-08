using System;
using System.Drawing;
using System.Windows.Forms;

namespace SystemTrayTimer
{
    public class FloatingWindow:Form
    {
        private string currentTime = "";
        private Font timeFont; // 新增：字体对象
        //private Label timeLabel;
        private bool editMode;
        private Point dragStartPoint;
        private bool isDragging;

        // 添加位置变化事件
        private Point lastSavedPosition;
        


        public FloatingWindow()
        {
            InitializeComponents();
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.Magenta;  // 选择任意颜色作为透明色
            this.TransparencyKey = Color.Magenta;  // 将该颜色设为透明
            this.ShowInTaskbar = false;

            // 启用双缓冲减少闪烁
            this.DoubleBuffered = true;

            // 初始化字体

            timeFont = new Font("微软雅黑", 12, FontStyle.Bold);

            // 初始化默认样式
            SetEditMode(false);
            //timeLabel.Visible = editMode; // 切换Label可见性

            this.StartPosition = FormStartPosition.Manual; // 强制手动定位
            this.AutoScaleMode = AutoScaleMode.Dpi; // 添加DPI自适应

        }
        protected override void SetBoundsCore(int x, int y,
        int width, int height, BoundsSpecified specified)
        {
            // 阻止窗体自动调整位置
            base.SetBoundsCore(x, y, width, height, specified);
        }
        protected override void OnLocationChanged(EventArgs e)
        {
            SavePositionToSettings(); // 直接保存
            base.OnLocationChanged(e);

        }


        // 新增代码：重写窗体样式
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                if (!editMode)
                {
                    // 添加透明鼠标穿透样式（0x20 = WS_EX_TRANSPARENT）
                    cp.ExStyle |= 0x20;
                }
                return cp;
            }
        }

        private void InitializeComponents()
        {
            // 移除所有Label相关代码
            this.Size = new Size(150, 50);
        }

        


        // 添加位置保存方法
        public void SavePositionToSettings()
        {
            if (this.Location != lastSavedPosition)
            {
                lastSavedPosition = this.Location;
                Properties.Settings.Default.FloatingWindowLocation = this.Location;
                Properties.Settings.Default.Save();
            }
        }
        public void SetTime(string time)
        {
            currentTime = time;
            //timeLabel.Text = time;
            this.Invalidate(); // 触发重绘更新自定义文字
        }

        //        public void SetEditMode(bool enable)
        //        {

        //            if (editMode == enable) return;
        //            editMode = enable;

        //            timeLabel.Visible = editMode; // 切换Label可见性


        //            // 切换窗体样式时需要重新创建句柄
        //            if (IsHandleCreated)
        //            {
        //                this.RecreateHandle();
        //            }

        //            // 通过边框和光标显示编辑状态
        //            if (enable)
        //            {
        //                this.Cursor = Cursors.SizeAll;
        //                this.Padding = new Padding(2);  // 为边框留出空间
        //                timeLabel.ForeColor = Color.Yellow;  // 编辑状态改变文字颜色

        //            }
        //            else
        //            {
        //                this.Cursor = Cursors.Default;
        //                this.Padding = new Padding(0);
        //                //timeLabel.ForeColor = Color.White;       // 设置默认文字颜色
        //;
        //            }

        //            // 强制重绘边框
        //            this.Invalidate();
        //        }
        //protected override void OnPaint(PaintEventArgs e)
        //{
        //    base.OnPaint(e);


        //    // 在编辑模式下绘制边框
        //    if (editMode)
        //    {
        //        using (var pen = new Pen(Color.Cyan, 2))
        //        {
        //            // 绘制内边框避免影响布局
        //            e.Graphics.DrawRectangle(
        //                pen,
        //                new Rectangle(
        //                    1, 1,
        //                    this.ClientSize.Width - 2,
        //                    this.ClientSize.Height - 2
        //                )
        //            );
        //        }

        //    }
        //    else
        //    {
        //        // 仅当非编辑模式时绘制文字
        //        DrawTextWithBorder(e.Graphics);
        //    }


        //}

        //绘制字体
        //private void DrawTextWithBorder(Graphics g)
        //{
        //    string text = timeLabel.Text;
        //    Font font = timeLabel.Font;
        //    Rectangle rect = this.ClientRectangle;

        //    // 设置文本格式
        //    StringFormat format = new StringFormat
        //    {
        //        Alignment = StringAlignment.Center,
        //        LineAlignment = StringAlignment.Center
        //    };

        //    // 绘制文本黑边
        //    using (GraphicsPath path = new GraphicsPath())
        //    {
        //        path.AddString(text, font.FontFamily, (int)font.Style,
        //            font.Size, rect, format);

        //        // 绘制轮廓
        //        using (Pen pen = new Pen(Color.Black, 4))
        //        {
        //            pen.LineJoin = LineJoin.Round;
        //            g.SmoothingMode = SmoothingMode.AntiAlias;
        //            g.DrawPath(pen, path);
        //        }

        //        // 填充文字
        //        using (Brush brush = new SolidBrush(Color.White))
        //        {
        //            g.FillPath(brush, path);
        //        }
        //    }

        //}

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // 只在有内容时绘制
            if (string.IsNullOrEmpty(currentTime)) return;

            // 设置文字格式
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;

            // 计算绘制区域（考虑可能的边框）
            RectangleF rect = new RectangleF(
                editMode ? 2 : 0,
                editMode ? 2 : 0,
                this.ClientSize.Width - (editMode ? 4 : 0),
                this.ClientSize.Height - (editMode ? 4 : 0));

            // 先绘制黑色描边
            using (Brush blackBrush = new SolidBrush(Color.Black))
            {
                // 八个方向偏移绘制黑边
                float offset = 1.2f;
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0) continue;
                        RectangleF shadowRect = new RectangleF(
                            rect.X + x * offset,
                            rect.Y + y * offset,
                            rect.Width,
                            rect.Height);
                        e.Graphics.DrawString(
                            currentTime,
                            timeFont,
                            blackBrush,
                            shadowRect,
                            format);
                    }
                }
            }

            // 再绘制白色文字
            using (Brush textBrush = new SolidBrush(editMode ? Color.Yellow : Color.White))
            {
                e.Graphics.DrawString(
                    currentTime,
                    timeFont,
                    textBrush,
                    rect,
                    format);
            }

            // 绘制编辑边框
            if (editMode)
            {
                using (Pen cyanPen = new Pen(Color.Cyan, 2))
                {
                    e.Graphics.DrawRectangle(
                        cyanPen,
                        1, 1,
                        this.ClientSize.Width - 2,
                        this.ClientSize.Height - 2);
                }
            }
        }
        public void SetEditMode(bool enable)
        {
            if (editMode == enable) return;
            editMode = enable;

            // 更新样式时需要重建窗口句柄
            if (IsHandleCreated)
                RecreateHandle();

            // 更新外观
            this.Cursor = editMode ? Cursors.SizeAll : Cursors.Default;
            this.Padding = editMode ? new Padding(2) : Padding.Empty;
            this.Invalidate();
        }
        
        //
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (editMode && e.Button == MouseButtons.Left)
            {
                // 捕获鼠标并记录起始位置
                isDragging = true;
                dragStartPoint = this.PointToScreen(e.Location);
                this.Capture = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (isDragging && editMode)
            {
                // 计算屏幕坐标差值
                Point currentPos = this.PointToScreen(e.Location);
                int deltaX = currentPos.X - dragStartPoint.X;
                int deltaY = currentPos.Y - dragStartPoint.Y;

                // 更新窗体位置
                this.Location = new Point(
                    this.Location.X + deltaX,
                    this.Location.Y + deltaY);

                // 更新起始位置
                dragStartPoint = currentPos;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (isDragging)
            {
                isDragging = false;
                this.Capture = false;
            }
        }

        // 释放字体资源
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (timeFont != null) timeFont.Dispose();
                this.Capture = false; // 新增释放鼠标捕获
            }
            base.Dispose(disposing);
        }
    }
}
