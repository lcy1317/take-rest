using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

[assembly: AssemblyTitle("Break Reminder")]
[assembly: AssemblyDescription("A lightweight full-screen break reminder")]
[assembly: AssemblyCompany("Local")]
[assembly: AssemblyProduct("Break Reminder")]
[assembly: AssemblyVersion("1.2.0.0")]
[assembly: AssemblyFileVersion("1.2.0.0")]

namespace BreakReminder
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            bool createdNew;
            using (Mutex mutex = new Mutex(true, "Local\\BreakReminder.SingleInstance", out createdNew))
            {
                if (!createdNew)
                {
                    MessageBox.Show(
                        "休息提醒已经在运行。",
                        "休息提醒",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new ReminderForm());
                GC.KeepAlive(mutex);
            }
        }
    }

    internal sealed class GradientCardPanel : Panel
    {
        public GradientCardPanel()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Rectangle bounds = ClientRectangle;
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            using (LinearGradientBrush background = new LinearGradientBrush(
                bounds,
                Color.FromArgb(25, 31, 52),
                Color.FromArgb(17, 23, 39),
                20F))
            {
                e.Graphics.FillRectangle(background, bounds);
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            float scaleX = Width / 360F;
            float scaleY = Height / 180F;
            using (SolidBrush blueGlow = new SolidBrush(Color.FromArgb(22, 58, 174, 255)))
            using (SolidBrush violetGlow = new SolidBrush(Color.FromArgb(18, 137, 92, 246)))
            {
                e.Graphics.FillEllipse(blueGlow, -65F * scaleX, -85F * scaleY, 210F * scaleX, 180F * scaleY);
                e.Graphics.FillEllipse(violetGlow, Width - (120F * scaleX), Height - (105F * scaleY), 185F * scaleX, 170F * scaleY);
            }

            using (Pen topHighlight = new Pen(Color.FromArgb(45, 255, 255, 255), 1F))
            {
                e.Graphics.DrawLine(topHighlight, 20F * scaleX, 0, Math.Max(20F * scaleX, Width - (20F * scaleX)), 0);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Pen border = new Pen(Color.FromArgb(52, 255, 255, 255), 1F))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawRectangle(border, 0, 0, Math.Max(0, Width - 1), Math.Max(0, Height - 1));
            }
        }
    }

    internal sealed class FocusIcon : Control
    {
        public FocusIcon()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor |
                ControlStyles.UserPaint,
                true);
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle circle = new Rectangle(1, 1, Width - 3, Height - 3);
            using (LinearGradientBrush fill = new LinearGradientBrush(
                circle,
                Color.FromArgb(80, 216, 255),
                Color.FromArgb(112, 111, 255),
                45F))
            {
                e.Graphics.FillEllipse(fill, circle);
            }

            float clockRadius = Math.Max(6F, Width * 0.19F);
            using (Pen clock = new Pen(Color.White, Math.Max(1.5F, Width / 21F)))
            {
                clock.StartCap = LineCap.Round;
                clock.EndCap = LineCap.Round;
                int cx = Width / 2;
                int cy = Height / 2;
                e.Graphics.DrawEllipse(clock, cx - clockRadius, cy - clockRadius, clockRadius * 2F, clockRadius * 2F);
                e.Graphics.DrawLine(clock, cx, cy, cx, cy - (clockRadius * 0.62F));
                e.Graphics.DrawLine(clock, cx, cy, cx + (clockRadius * 0.5F), cy + (clockRadius * 0.25F));
            }
        }
    }

    internal sealed class RoundedActionButton : Button
    {
        private bool isHovered;
        private bool isPressed;

        public RoundedActionButton()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor |
                ControlStyles.UserPaint,
                true);
            BackColor = Color.Transparent;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
            TabStop = false;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            isHovered = false;
            isPressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            if (mevent.Button == MouseButtons.Left)
            {
                isPressed = true;
                Invalidate();
            }
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            isPressed = false;
            Invalidate();
            base.OnMouseUp(mevent);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (Width <= 2 || Height <= 2)
            {
                return;
            }

            Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = MakeRoundPath(bounds, Math.Max(8, bounds.Height / 4)))
            {
                Region = new Region(path);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            Color start = isPressed
                ? Color.FromArgb(47, 154, 224)
                : (isHovered ? Color.FromArgb(92, 212, 255) : Color.FromArgb(75, 192, 246));
            Color end = isPressed
                ? Color.FromArgb(94, 73, 211)
                : (isHovered ? Color.FromArgb(128, 105, 255) : Color.FromArgb(109, 89, 238));

            using (GraphicsPath path = MakeRoundPath(bounds, Math.Max(8, bounds.Height / 4)))
            using (LinearGradientBrush fill = new LinearGradientBrush(bounds, start, end, 20F))
            using (Pen edge = new Pen(Color.FromArgb(65, 255, 255, 255), 1F))
            {
                e.Graphics.FillPath(fill, path);
                e.Graphics.DrawPath(edge, path);
            }

            TextRenderer.DrawText(
                e.Graphics,
                Text,
                Font,
                bounds,
                Color.White,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine);
        }

        private static GraphicsPath MakeRoundPath(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            GraphicsPath path = new GraphicsPath();
            Rectangle arc = new Rectangle(bounds.Left, bounds.Top, diameter, diameter);
            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    internal sealed class FocusProgressBar : Control
    {
        private double progress;

        public double Progress
        {
            get { return progress; }
            set
            {
                progress = Math.Max(0D, Math.Min(1D, value));
                Invalidate();
            }
        }

        public FocusProgressBar()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor |
                ControlStyles.UserPaint,
                true);
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle track = new Rectangle(0, 0, Width - 1, Height - 1);
            if (track.Width <= 0 || track.Height <= 0)
            {
                return;
            }

            using (GraphicsPath trackPath = MakeCapsule(track))
            using (SolidBrush trackFill = new SolidBrush(Color.FromArgb(42, 255, 255, 255)))
            {
                e.Graphics.FillPath(trackFill, trackPath);
            }

            int fillWidth = Math.Max(track.Height, (int)Math.Round(track.Width * progress));
            if (progress > 0D)
            {
                Rectangle fillBounds = new Rectangle(0, 0, Math.Min(track.Width, fillWidth), track.Height);
                using (GraphicsPath fillPath = MakeCapsule(fillBounds))
                using (LinearGradientBrush fill = new LinearGradientBrush(
                    fillBounds,
                    Color.FromArgb(72, 207, 255),
                    Color.FromArgb(122, 97, 255),
                    0F))
                {
                    e.Graphics.FillPath(fill, fillPath);
                }
            }
        }

        private static GraphicsPath MakeCapsule(Rectangle bounds)
        {
            int diameter = Math.Max(1, bounds.Height);
            GraphicsPath path = new GraphicsPath();
            if (bounds.Width <= diameter)
            {
                path.AddEllipse(bounds);
                return path;
            }

            Rectangle left = new Rectangle(bounds.Left, bounds.Top, diameter, diameter);
            Rectangle right = new Rectangle(bounds.Right - diameter, bounds.Top, diameter, diameter);
            path.AddArc(left, 90, 180);
            path.AddArc(right, 270, 180);
            path.CloseFigure();
            return path;
        }
    }

    internal sealed class ReminderForm : Form
    {
        private const int FloatingWidth = 360;
        private const int FloatingHeight = 180;
        private const int MinimumIntervalSeconds = 15 * 60;
        private const int MaximumIntervalSeconds = 20 * 60;
        private const int BreakSeconds = 60;

        private readonly Random random = new Random();
        private readonly System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        private readonly GradientCardPanel floatingPanel = new GradientCardPanel();
        private readonly TableLayoutPanel breakPanel = new TableLayoutPanel();
        private readonly Label nextBreakLabel = new Label();
        private readonly Label remainingLabel = new Label();
        private readonly Label countdownLabel = new Label();
        private readonly FocusProgressBar focusProgress = new FocusProgressBar();
        private readonly NotifyIcon notifyIcon = new NotifyIcon();
        private readonly ContextMenuStrip menu = new ContextMenuStrip();
        private readonly ToolTip toolTip = new ToolTip();

        private DateTime nextBreakAt;
        private DateTime breakEndsAt;
        private int scheduledIntervalSeconds;
        private bool isBreakActive;
        private bool isHiddenToTray;
        private bool hasShownTrayTip;
        private bool forceExit;
        private bool isDragging;
        private Point dragStartCursor;
        private Point dragStartWindow;
        private Rectangle floatingBounds;
        private Size floatingSize;
        private Icon trayIcon;

        public ReminderForm()
        {
            SuspendLayout();

            Text = "休息提醒";
            ClientSize = new Size(FloatingWidth, FloatingHeight);
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            BackColor = Color.FromArgb(23, 29, 41);
            ForeColor = Color.White;
            TopMost = true;
            ShowInTaskbar = false;
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            DoubleBuffered = true;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

            BuildMenu();
            BuildFloatingPanel();
            BuildBreakPanel();

            Controls.Add(breakPanel);
            Controls.Add(floatingPanel);
            ContextMenuStrip = menu;

            timer.Interval = 200;
            timer.Tick += TimerTick;
            Shown += FormShown;
            KeyDown += FormKeyDown;
            FormClosing += FormIsClosing;

            ResumeLayout(false);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int DropShadow = 0x00020000;
                CreateParams parameters = base.CreateParams;
                parameters.ClassStyle |= DropShadow;
                return parameters;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (isBreakActive)
            {
                return;
            }

            using (Pen border = new Pen(Color.FromArgb(65, 77, 99), 1F))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawRectangle(border, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (!isBreakActive && Width > 20 && Height > 20)
            {
                int radius = Math.Max(18, (int)Math.Round(18D * Width / FloatingWidth));
                using (GraphicsPath path = CreateRoundedRectangle(new Rectangle(0, 0, Width, Height), radius))
                {
                    Region = new Region(path);
                }
            }
        }

        private void BuildMenu()
        {
            ToolStripMenuItem showWindow = new ToolStripMenuItem("显示悬浮窗");
            showWindow.Font = new Font(showWindow.Font, FontStyle.Bold);
            showWindow.Click += delegate { ShowFloatingFromTray(); };

            ToolStripMenuItem testNow = new ToolStripMenuItem("立即休息（1 分钟）");
            testNow.Click += delegate { BeginBreak(); };

            ToolStripMenuItem reset = new ToolStripMenuItem("重新开始计时");
            reset.Click += delegate
            {
                if (isBreakActive)
                {
                    ReturnToFloating();
                }
                else
                {
                    ScheduleNextBreak();
                }
            };

            ToolStripMenuItem exit = new ToolStripMenuItem("退出");
            exit.Click += delegate { ExitApplication(); };

            menu.Items.Add(showWindow);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(testNow);
            menu.Items.Add(reset);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(exit);

            trayIcon = CreateTrayIcon();
            notifyIcon.Icon = trayIcon;
            notifyIcon.Text = "休息提醒：运行中";
            notifyIcon.ContextMenuStrip = menu;
            notifyIcon.Visible = true;
            notifyIcon.DoubleClick += delegate
            {
                ShowFloatingFromTray();
            };
        }

        private void BuildFloatingPanel()
        {
            floatingPanel.Dock = DockStyle.Fill;
            floatingPanel.BackColor = Color.Transparent;
            floatingPanel.ContextMenuStrip = menu;

            FocusIcon icon = new FocusIcon();
            icon.Location = new Point(18, 17);
            icon.Size = new Size(42, 42);

            Label title = new Label();
            title.AutoSize = true;
            title.Location = new Point(72, 15);
            title.Font = new Font(Font.FontFamily, 11.5F, FontStyle.Bold);
            title.ForeColor = Color.FromArgb(236, 241, 249);
            title.BackColor = Color.Transparent;
            title.Text = "专注守护";

            nextBreakLabel.AutoSize = false;
            nextBreakLabel.Location = new Point(72, 39);
            nextBreakLabel.Size = new Size(210, 20);
            nextBreakLabel.Font = new Font(Font.FontFamily, 8.5F, FontStyle.Regular);
            nextBreakLabel.ForeColor = Color.FromArgb(150, 164, 188);
            nextBreakLabel.BackColor = Color.Transparent;
            nextBreakLabel.Text = "正在安排下一次休息…";

            Label status = new Label();
            status.AutoSize = true;
            status.Location = new Point(18, 76);
            status.Font = new Font(Font.FontFamily, 8.5F, FontStyle.Regular);
            status.ForeColor = Color.FromArgb(130, 146, 172);
            status.BackColor = Color.Transparent;
            status.Text = "距离休息";

            remainingLabel.AutoSize = false;
            remainingLabel.Location = new Point(17, 91);
            remainingLabel.Size = new Size(165, 48);
            remainingLabel.Font = new Font("Segoe UI Semibold", 28F, FontStyle.Regular, GraphicsUnit.Point);
            remainingLabel.ForeColor = Color.FromArgb(239, 244, 252);
            remainingLabel.BackColor = Color.Transparent;
            remainingLabel.Text = "15:00";

            RoundedActionButton restNowButton = new RoundedActionButton();
            restNowButton.Location = new Point(238, 84);
            restNowButton.Size = new Size(104, 49);
            restNowButton.Font = new Font(Font.FontFamily, 10F, FontStyle.Bold);
            restNowButton.Text = "立即休息";
            restNowButton.Click += delegate { BeginBreak(); };

            focusProgress.Location = new Point(18, 155);
            focusProgress.Size = new Size(324, 7);
            focusProgress.Progress = 0D;

            Button closeButton = new Button();
            closeButton.Location = new Point(FloatingWidth - 39, 10);
            closeButton.Size = new Size(28, 28);
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(45, 255, 255, 255);
            closeButton.BackColor = Color.Transparent;
            closeButton.ForeColor = Color.FromArgb(147, 161, 184);
            closeButton.Font = new Font("Segoe UI", 12F, FontStyle.Regular);
            closeButton.Text = "×";
            closeButton.TabStop = false;
            closeButton.Cursor = Cursors.Hand;
            closeButton.Click += delegate { ExitApplication(); };
            toolTip.SetToolTip(closeButton, "退出程序");

            Button minimizeButton = new Button();
            minimizeButton.Location = new Point(FloatingWidth - 72, 10);
            minimizeButton.Size = new Size(28, 28);
            minimizeButton.FlatStyle = FlatStyle.Flat;
            minimizeButton.FlatAppearance.BorderSize = 0;
            minimizeButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(45, 255, 255, 255);
            minimizeButton.BackColor = Color.Transparent;
            minimizeButton.ForeColor = Color.FromArgb(147, 161, 184);
            minimizeButton.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
            minimizeButton.Text = "—";
            minimizeButton.TabStop = false;
            minimizeButton.Cursor = Cursors.Hand;
            minimizeButton.Click += delegate { HideToTray(); };
            toolTip.SetToolTip(minimizeButton, "最小化到系统托盘");
            toolTip.SetToolTip(restNowButton, "立刻开始 1 分钟休息");

            floatingPanel.Controls.Add(icon);
            floatingPanel.Controls.Add(title);
            floatingPanel.Controls.Add(nextBreakLabel);
            floatingPanel.Controls.Add(status);
            floatingPanel.Controls.Add(remainingLabel);
            floatingPanel.Controls.Add(restNowButton);
            floatingPanel.Controls.Add(focusProgress);
            floatingPanel.Controls.Add(minimizeButton);
            floatingPanel.Controls.Add(closeButton);

            AttachDragHandlers(floatingPanel);
            AttachDragHandlers(icon);
            AttachDragHandlers(title);
            AttachDragHandlers(nextBreakLabel);
            AttachDragHandlers(status);
            AttachDragHandlers(remainingLabel);
        }

        private void BuildBreakPanel()
        {
            breakPanel.Dock = DockStyle.Fill;
            breakPanel.BackColor = Color.FromArgb(8, 15, 27);
            breakPanel.ColumnCount = 1;
            breakPanel.RowCount = 5;
            breakPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            breakPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 24F));
            breakPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 17F));
            breakPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 27F));
            breakPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 13F));
            breakPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 19F));
            breakPanel.Visible = false;

            Label heading = MakeCenteredLabel("休息一下", 30F, FontStyle.Bold, Color.FromArgb(235, 242, 252));
            heading.Dock = DockStyle.Fill;

            countdownLabel.Dock = DockStyle.Fill;
            countdownLabel.TextAlign = ContentAlignment.MiddleCenter;
            countdownLabel.Font = new Font("Segoe UI Light", 70F, FontStyle.Regular, GraphicsUnit.Point);
            countdownLabel.ForeColor = Color.FromArgb(93, 206, 255);
            countdownLabel.Text = "01:00";

            Label hint = MakeCenteredLabel("看看远处，放松眼睛和肩颈", 15F, FontStyle.Regular, Color.FromArgb(165, 180, 201));
            hint.Dock = DockStyle.Fill;

            Label exitHint = MakeCenteredLabel("倒计时结束后自动恢复", 9F, FontStyle.Regular, Color.FromArgb(91, 105, 126));
            exitHint.Dock = DockStyle.Fill;

            RoundedActionButton endBreakButton = new RoundedActionButton();
            endBreakButton.Anchor = AnchorStyles.None;
            endBreakButton.Size = new Size(170, 42);
            endBreakButton.MinimumSize = new Size(170, 42);
            endBreakButton.Font = new Font(Font.FontFamily, 9.5F, FontStyle.Bold);
            endBreakButton.Text = "提前结束休息  Esc";
            endBreakButton.Click += delegate { ReturnToFloating(); };

            TableLayoutPanel footer = new TableLayoutPanel();
            footer.Dock = DockStyle.Fill;
            footer.BackColor = Color.Transparent;
            footer.ColumnCount = 1;
            footer.RowCount = 2;
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            footer.RowStyles.Add(new RowStyle(SizeType.Percent, 62F));
            footer.RowStyles.Add(new RowStyle(SizeType.Percent, 38F));
            footer.Controls.Add(endBreakButton, 0, 0);
            footer.Controls.Add(exitHint, 0, 1);

            breakPanel.Controls.Add(new Panel(), 0, 0);
            breakPanel.Controls.Add(heading, 0, 1);
            breakPanel.Controls.Add(countdownLabel, 0, 2);
            breakPanel.Controls.Add(hint, 0, 3);
            breakPanel.Controls.Add(footer, 0, 4);
        }

        private Label MakeCenteredLabel(string text, float size, FontStyle style, Color color)
        {
            Label label = new Label();
            label.Text = text;
            label.TextAlign = ContentAlignment.MiddleCenter;
            label.Font = new Font(Font.FontFamily, size, style, GraphicsUnit.Point);
            label.ForeColor = color;
            label.BackColor = Color.Transparent;
            return label;
        }

        private void FormShown(object sender, EventArgs e)
        {
            floatingSize = ClientSize;
            PlaceAtBottomRight();
            ScheduleNextBreak();
            timer.Start();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            if (isBreakActive)
            {
                TimeSpan remaining = breakEndsAt - now;
                if (remaining <= TimeSpan.Zero)
                {
                    ReturnToFloating();
                    return;
                }

                int seconds = Math.Max(0, (int)Math.Ceiling(remaining.TotalSeconds));
                countdownLabel.Text = String.Format("{0:00}:{1:00}", seconds / 60, seconds % 60);
                return;
            }

            if (now >= nextBreakAt)
            {
                BeginBreak();
                return;
            }

            TimeSpan untilBreak = nextBreakAt - now;
            int secondsLeft = Math.Max(0, (int)Math.Ceiling(untilBreak.TotalSeconds));
            remainingLabel.Text = String.Format("{0:00}:{1:00}", secondsLeft / 60, secondsLeft % 60);
            focusProgress.Progress = 1D - (untilBreak.TotalSeconds / scheduledIntervalSeconds);
            notifyIcon.Text = String.Format("休息提醒：{0:00}:{1:00} 后休息", secondsLeft / 60, secondsLeft % 60);
        }

        private void ScheduleNextBreak()
        {
            scheduledIntervalSeconds = random.Next(MinimumIntervalSeconds, MaximumIntervalSeconds + 1);
            nextBreakAt = DateTime.Now.AddSeconds(scheduledIntervalSeconds);
            nextBreakLabel.Text = "下次休息  ·  " + nextBreakAt.ToString("HH:mm");
            focusProgress.Progress = 0D;
            UpdateFloatingCountdownImmediately();
        }

        private void UpdateFloatingCountdownImmediately()
        {
            int secondsLeft = Math.Max(0, (int)Math.Ceiling((nextBreakAt - DateTime.Now).TotalSeconds));
            remainingLabel.Text = String.Format("{0:00}:{1:00}", secondsLeft / 60, secondsLeft % 60);
        }

        private void HideToTray()
        {
            if (isBreakActive)
            {
                return;
            }

            floatingBounds = Bounds;
            isHiddenToTray = true;
            Hide();

            if (!hasShownTrayTip)
            {
                hasShownTrayTip = true;
                notifyIcon.ShowBalloonTip(
                    2500,
                    "休息提醒仍在运行",
                    "双击托盘图标可恢复悬浮窗。",
                    ToolTipIcon.Info);
            }
        }

        private void ShowFloatingFromTray()
        {
            isHiddenToTray = false;
            Show();

            if (isBreakActive)
            {
                ShowInTaskbar = true;
                BringToFront();
                Activate();
                return;
            }

            WindowState = FormWindowState.Normal;
            ShowInTaskbar = false;
            if (IsOnAnyScreen(floatingBounds))
            {
                Bounds = floatingBounds;
            }
            else
            {
                PlaceAtBottomRight();
            }
            BringToFront();
            Activate();
        }

        private Icon CreateTrayIcon()
        {
            using (Bitmap bitmap = new Bitmap(32, 32, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.Clear(Color.Transparent);
                Rectangle circle = new Rectangle(2, 2, 27, 27);
                using (LinearGradientBrush fill = new LinearGradientBrush(
                    circle,
                    Color.FromArgb(66, 202, 255),
                    Color.FromArgb(118, 92, 244),
                    45F))
                using (Pen clock = new Pen(Color.White, 2.3F))
                {
                    clock.StartCap = LineCap.Round;
                    clock.EndCap = LineCap.Round;
                    graphics.FillEllipse(fill, circle);
                    graphics.DrawEllipse(clock, 9, 9, 13, 13);
                    graphics.DrawLine(clock, 15.5F, 15.5F, 15.5F, 11.5F);
                    graphics.DrawLine(clock, 15.5F, 15.5F, 19F, 17.5F);
                }

                IntPtr handle = bitmap.GetHicon();
                try
                {
                    using (Icon temporary = Icon.FromHandle(handle))
                    {
                        return (Icon)temporary.Clone();
                    }
                }
                finally
                {
                    DestroyIcon(handle);
                }
            }
        }

        private void FormKeyDown(object sender, KeyEventArgs e)
        {
            if (isBreakActive && e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                ReturnToFloating();
            }
        }

        private void ExitApplication()
        {
            forceExit = true;
            Close();
        }

        private void BeginBreak()
        {
            if (isBreakActive)
            {
                return;
            }

            floatingBounds = Bounds;
            Screen targetScreen = Screen.FromRectangle(floatingBounds);
            isBreakActive = true;
            Region = null;
            floatingPanel.Visible = false;
            breakPanel.Visible = true;
            breakPanel.BringToFront();
            BackColor = Color.FromArgb(8, 15, 27);
            ShowInTaskbar = true;
            Show();
            Bounds = targetScreen.Bounds;
            TopMost = true;
            countdownLabel.Text = "01:00";
            breakEndsAt = DateTime.Now.AddSeconds(BreakSeconds);
            notifyIcon.Text = "休息提醒：正在休息 01:00";
            BringToFront();
            Activate();
        }

        private void ReturnToFloating()
        {
            isBreakActive = false;
            breakPanel.Visible = false;
            floatingPanel.Visible = true;
            floatingPanel.BringToFront();
            BackColor = Color.FromArgb(23, 29, 41);
            ShowInTaskbar = false;
            ClientSize = floatingSize;

            if (IsOnAnyScreen(floatingBounds))
            {
                Bounds = floatingBounds;
            }
            else
            {
                PlaceAtBottomRight();
            }

            ScheduleNextBreak();
            if (isHiddenToTray)
            {
                Hide();
            }
            else
            {
                Show();
                BringToFront();
            }
            Invalidate();
        }

        private bool IsOnAnyScreen(Rectangle rectangle)
        {
            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.WorkingArea.IntersectsWith(rectangle))
                {
                    return true;
                }
            }
            return false;
        }

        private void PlaceAtBottomRight()
        {
            Rectangle area = Screen.PrimaryScreen.WorkingArea;
            int margin = Math.Max(24, (int)Math.Round(24D * floatingSize.Width / FloatingWidth));
            Bounds = new Rectangle(
                area.Right - floatingSize.Width - margin,
                area.Bottom - floatingSize.Height - margin,
                floatingSize.Width,
                floatingSize.Height);
            floatingBounds = Bounds;
        }

        private void AttachDragHandlers(Control control)
        {
            control.MouseDown += DragMouseDown;
            control.MouseMove += DragMouseMove;
            control.MouseUp += DragMouseUp;
        }

        private void DragMouseDown(object sender, MouseEventArgs e)
        {
            if (isBreakActive || e.Button != MouseButtons.Left)
            {
                return;
            }

            isDragging = true;
            dragStartCursor = Cursor.Position;
            dragStartWindow = Location;
            ((Control)sender).Capture = true;
        }

        private void DragMouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging || isBreakActive || e.Button != MouseButtons.Left)
            {
                return;
            }

            Point cursor = Cursor.Position;
            Location = new Point(
                dragStartWindow.X + cursor.X - dragStartCursor.X,
                dragStartWindow.Y + cursor.Y - dragStartCursor.Y);
        }

        private void DragMouseUp(object sender, MouseEventArgs e)
        {
            if (!isDragging)
            {
                return;
            }

            isDragging = false;
            ((Control)sender).Capture = false;
            floatingBounds = Bounds;
        }

        private GraphicsPath CreateRoundedRectangle(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Rectangle arc = new Rectangle(bounds.Location, new Size(diameter, diameter));
            GraphicsPath path = new GraphicsPath();
            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void FormIsClosing(object sender, FormClosingEventArgs e)
        {
            if (isBreakActive && !forceExit && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                ReturnToFloating();
                return;
            }

            timer.Stop();
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            if (trayIcon != null)
            {
                trayIcon.Dispose();
                trayIcon = null;
            }
            toolTip.Dispose();
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr handle);
    }
}
