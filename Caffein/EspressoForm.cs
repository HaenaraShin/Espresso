using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Espresso
{
    public partial class EspressoForm : Form
    {
        private bool isCoffeeOn = false;
        private bool flagExit = false;

        public EspressoForm()
        {
            InitializeComponent();
            this.FormClosing += Form1_FormClosing;
            this.notifyIcon1.DoubleClick += notifyIcon1_DoubleClick;

            this.notifyIcon1.MouseClick += notifyIcon1_Click;
            this.ExitToolStripMenuItem.Click += ExitToolStripMenuItem_Click;
            this.OpenToolStripMenuItem.Click += OpenToolStripMenuItem_Click;

            //Win32.PreventScreenAndSleep();
            coffeeOn();

            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(
                                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            if (registryKey.GetValue("MyApp") != null)
            {
                checkStartRun.Checked = true;
            }
        }
        private void Form1_Load(object sender, System.EventArgs e)
        {
            //  지우면 안됌
        }

        // 트레이에 아이콘을 넣고 윈도우폼을 닫아도 종료되지 않도록 설정하는 코드는 
        // http://crynut84.tistory.com/41 참고

        // 트레이의 종료 메뉴를 눌렀을때
        void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            flagExit = true;
            Win32.AllowMonitorPowerdown();
            //트레이아이콘 없앰
            notifyIcon1.Visible = false;
            //프로세스 종료
            Application.Exit();
            //this.Close();
        }

        void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Visible = true; // 폼의 표시
            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal; // 최소화를 멈춘다 
            this.Activate(); // 폼을 활성화 시킨다
        }

        //트레이 아이콘을 더블클릭 했을시 호출
        void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Visible = true; // 폼의 표시
            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal; // 최소화를 멈춘다 
            this.Activate(); // 폼을 활성화 시킨다
        }

        //폼이 종료 되려 할때 호출
        void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!flagExit)
            {
                e.Cancel = true; // 종료 이벤트를 취소 시킨다
                notifyIcon1.BalloonTipTitle = "Espresso";
                notifyIcon1.BalloonTipText = "You have not closed this application. It has be moved to the tray. Right click the Icon to exit.";
                notifyIcon1.ShowBalloonTip(5000);
            }
            this.Visible = false; // 폼을 표시하지 않는다;
        }


        //트레이 아이콘을 클릭 했을시 호출
        void notifyIcon1_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            if (me.Button == MouseButtons.Left)
            {
                coffeeOnOff();
            }
        }

        public void coffeeOn()
        {
            if(isCoffeeOn)
            {
                // 이미 켜져있다면 다시 켤 이유가 없다.
                return;
            }
            
            Win32.PreventScreenAndSleep();
            notifyIcon1.Icon = Properties.Resources.Coffee_On_icon;
            pictureBox1.Image = Properties.Resources.Coffee_On;
            OnOffLabel.Text = "Coffee Activated";
            notifyIcon1.BalloonTipTitle = "Espresso";
            notifyIcon1.BalloonTipText = "Now Espresso keeps your PC awake.";
            notifyIcon1.ShowBalloonTip(3000);
            isCoffeeOn = true;
        }

        public void coffeeOff()
        {
            if (!isCoffeeOn)
            {
                // 이미 꺼져있다면 다시 켤 이유가 없다.
                return;
            }
            Win32.AllowMonitorPowerdown();
            notifyIcon1.Icon = Properties.Resources.Coffee_Off_icon;
            pictureBox1.Image = Properties.Resources.Coffee_Off;
            OnOffLabel.Text = "Coffee Not Activated";
            notifyIcon1.BalloonTipTitle = "Espresso";
            notifyIcon1.BalloonTipText = "Now your PC may turn into power saving mode.";
            notifyIcon1.ShowBalloonTip(3000);
            isCoffeeOn = false;
        }

        public void coffeeOnOff()
        {
            if (isCoffeeOn) coffeeOff();
            else coffeeOn();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            coffeeOnOff();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void checkStartRun_CheckedChanged(object sender, EventArgs e)
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(
                                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            if (checkStartRun.Checked)
            {
                // 윈도우 시작시 실행하도록 레지스트리 등록
                //레지스트리 등록 할때
                if (registryKey.GetValue("MyApp") == null)
                {
                    registryKey.SetValue("MyApp", Application.ExecutablePath.ToString());
                }
                
            }
            else
            {
                // 윈도우 시작시 실행하지 않도록 레지스트리 삭제
                if (registryKey.GetValue("MyApp") != null)
                {
                    registryKey.DeleteValue("MyApp", false);
                }
            }
        }

        private void LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://heavybros.dothome.co.kr");
        }
    }

    // 절전모드에 빠지지 않도록 설정하는 코드는 http://msdn.microsoft.com/en-us/library/windows/desktop/aa373208(v=vs.85).aspx 참고

    public class Win32
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);
        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
            // Legacy flag, should not be used.
            // ES_USER_PRESENT = 0x00000004
        }
        public static void PreventScreenAndSleep()
        {
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS |
                                    EXECUTION_STATE.ES_SYSTEM_REQUIRED |
                                    EXECUTION_STATE.ES_AWAYMODE_REQUIRED |
                                    EXECUTION_STATE.ES_DISPLAY_REQUIRED);
        }
        public static void AllowMonitorPowerdown()
        {
            Console.WriteLine(SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS));
        }
    }


}
