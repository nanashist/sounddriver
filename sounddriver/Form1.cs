using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NAudio;
using NAudio.Wave;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SoundDriver.Init();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            SoundDriver.MainFileLoad(textBox1.Text);

            SoundDriver.Play();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            lblPlaybackState.Text = SoundDriver.PlaybackState().ToString();

        }

        private Sound.ScratchSound scratchsound;
        private bool inplay = false;

        /// <summary>
        /// すく再生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            inplay = !inplay;
            if (scratchsound != null)
            {
                scratchsound.stopflg = !inplay;
            }
            if (inplay)
            {

                    if (scratchsound == null)
                    {
                        if (SoundDriver.GetWaveProvider("main") != null)
                        {
                            scratchsound = new Sound.ScratchSound(SoundDriver.GetWaveProvider("main").WaveFormat);
                            SoundDriver.AddWaveProvider(scratchsound.OutputWaveProvider, "stream");
                        }
                        else{
                            scratchsound = new Sound.ScratchSound();
                            SoundDriver.AddWaveProvider(scratchsound.OutputWaveProvider, "stream");
                            SoundDriver.Play();
                        }
                    }
                    //音声ファイル追加&インデックスを変更。既に追加済みならインデックスのみ変更
                    scratchsound.addscratch(textBox2.Text);
                    pictureBox1.Image = (Image)scratchsound.ScratchBitmap(200, 200);
                    scratchsound.speed = (double)trackBar1.Value;
                    lblDebug.Text = "再生";
                    scratchsound.TestMake();

                    lblDebug.Text = "停止";
                    inplay = false;

            }
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(trackBar1, trackBar1.Value.ToString());
            if (scratchsound != null)
            {
                scratchsound.speed = (double)trackBar1.Value;
            }
        }

        private double speed = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            /*
            for (int i = 1; i <= 256; i++)
            {
                if (KeyBoard.KeyInput.GetKeyState(i))
                {
                    System.Diagnostics.Debug.WriteLine(
                        "キーボード押下:" + ((Keys)i).ToString());
                }
            }
             */
            if (scratchsound != null)
            {
                List<Keys> keylist = KeyBoard.KeyInput.GetPressedKeyList();
                bool bSpeed = false;
                foreach (Keys k in keylist)
                {
                    if (k == Keys.A) scratchsound.TestOneFrameMake(0.6);
                    if (k == Keys.S) scratchsound.TestOneFrameMake(0.7);
                    if (k == Keys.D) scratchsound.TestOneFrameMake(0.8);
                    if (k == Keys.F) scratchsound.TestOneFrameMake(0.9);
                    if (k == Keys.G) scratchsound.TestOneFrameMake(1);
                    if (k == Keys.Z) bSpeed = true;
                }
                if (bSpeed)
                    speed += Convert.ToDouble( txtSpeed.Text);
                else
                    speed -= Convert.ToDouble(txtSpeed.Text);
                if (speed < 0) speed = 0;
                if (speed > 1) speed = 1;
                if (speed >= 0.5)
                {
                    scratchsound.TestOneFrameMake(speed);
                }
                scratchsound.TestStreaming();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (scratchsound != null)
            {
                SoundDriver.Stop();
            }
            Test.Print("終了");
            Test.Close();
        }

        private void btnStreaming_Click(object sender, EventArgs e)
        {
            if (scratchsound == null)
            {
                if (SoundDriver.GetWaveProvider("main") != null)
                {
                    scratchsound = new Sound.ScratchSound(SoundDriver.GetWaveProvider("main").WaveFormat);
                    SoundDriver.AddWaveProvider(scratchsound.OutputWaveProvider, "stream");
                }
                else
                {
                    scratchsound = new Sound.ScratchSound();
                    SoundDriver.AddWaveProvider(scratchsound.OutputWaveProvider, "stream");
                    SoundDriver.Play();
                }
            }
            //音声ファイル追加&インデックスを変更。既に追加済みならインデックスのみ変更
            scratchsound.addscratch(textBox2.Text);
            pictureBox1.Image = (Image)scratchsound.ScratchBitmap(200, 200);

        }


    }
}
