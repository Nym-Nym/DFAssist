﻿using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace App
{
    public partial class OverlayForm : Form
    {
        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        static extern bool ReleaseCapture();

        const int WS_EX_LAYERED = 0x80000;
        const int WS_EX_TOOLWINDOW = 0x80;
        const int WM_NCLBUTTONDOWN = 0xA1;
        const int HT_CAPTION = 0x2;

        Timer timer = null;
        int blinkCount;

        internal OverlayForm()
        {
            InitializeComponent();

            timer = new Timer();
            timer.Interval = Global.BLINK_INTERVAL;
            timer.Tick += Timer_Tick;

            SetStatus(false);

            if (Settings.OverlayX == Global.OVERLAY_XY_UNSET || Settings.OverlayY == Global.OVERLAY_XY_UNSET)
            {
                StartPosition = FormStartPosition.CenterScreen;
            }
            else
            {
                StartPosition = FormStartPosition.Manual;
                Location = new Point(Settings.OverlayX, Settings.OverlayY);
            }
        }
        
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_LAYERED;
                cp.ExStyle |= WS_EX_TOOLWINDOW;
                return cp;
            }
        }

        private void panel_Move_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void OverlayForm_LocationChanged(object sender, EventArgs e)
        {
            Settings.OverlayX = Location.X;
            Settings.OverlayY = Location.Y;
            Settings.Save();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (++blinkCount > Global.BLINK_COUNT)
            {
                StopBlink();
            }
            else {
                if (BackColor == Color.Black)
                {
                    BackColor = Global.ACCENT_COLOR;
                }
                else
                {
                    BackColor = Color.Black;
                }
            }
        }

        internal void SetStatus(bool isOkay)
        {
            if (isOkay)
            {
                panel_Move.BackColor = Color.FromArgb(0, 64, 0);

                CancelDutyFinder();
            }
            else
            {
                panel_Move.BackColor = Color.FromArgb(64, 0, 0);

                CancelDutyFinder();
                label_DutyName.Text = "< 클라이언트 통신 대기중... >";
            }
        }

        internal void SetDutyCount(int dutyCount)
        {
            label_DutyCount.Text = string.Format("총 {0}개 임무 매칭중", dutyCount);
        }

        internal void SetDutyStatus(Instance instance, byte tank, byte dps, byte healer)
        {
            label_DutyName.Text = string.Format("< {0} >", instance.Name);
            label_DutyStatus.Text = string.Format("{0}/{3}    {1}/{4}    {2}/{5}", tank, healer, dps, instance.Tank, instance.Healer, instance.DPS);
        }

        internal void SetDutyAsMatched(Instance instance)
        {
            label_DutyCount.Text = "입장 확인 대기중";
            label_DutyName.Text = string.Format("< {0} >", instance.Name);
            label_DutyStatus.Text = "매칭!";

            StartBlink();
        }

        internal void CancelDutyFinder()
        {
            StopBlink();

            label_DutyCount.Text = "";
            label_DutyName.Text = "< 매칭중인 임무 없음 >";
            label_DutyStatus.Text = "";
        }

        internal void ResetFormLocation()
        {
            CenterToScreen();
        }

        private void StartBlink()
        {
            blinkCount = 0;
            timer.Start();
        }

        private void StopBlink()
        {
            timer.Stop();
            BackColor = Color.Black;
        }
    }
}
