using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class frmScratch : Form
    {
        Sound.ScratchSound scratchsound;

        public frmScratch()
        {
            InitializeComponent();
        }

        private void frmScratch_Load(object sender, EventArgs e)
        {
            //初期化とストリーミング開始
            SoundDriver.Init();
            if (scratchsound == null)
            scratchsound = new Sound.ScratchSound();
            SoundDriver.AddWaveProvider(scratchsound.OutputWaveProvider, "stream");
            SoundDriver.Play();
            tmrStreaming.Enabled = true;
        }

        #region "D&D"
        string _dropfilename = "";
        private void txtFilePath_DragDrop(object sender, DragEventArgs e)
        {
            if (_dropfilename != "")
            {
                txtFilePath.Text = _dropfilename.Replace("\\", "/");
                set_scratchfile(txtFilePath.Text);
            }
        }

        private void txtFilePath_DragEnter(object sender, DragEventArgs e)
        {
            _dropfilename = "";
            if (e.Data.GetData(DataFormats.FileDrop) != null)
            {
                string[] s = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (s.Count() == 1)
                {
                    _dropfilename = s[0];
                    e.Effect = DragDropEffects.Copy;
                    return;
                }
            }
            e.Effect = DragDropEffects.None;
        }
        #endregion

        string _filename="";
        int _que;
        /// <summary>
        /// ファイルセット
        /// </summary>
        /// <param name="filename"></param>
        private void set_scratchfile(string filename)
        {
            //再生中なら止める
            if(!string.IsNullOrEmpty(_filename))
            {
                //scratchsoundのメモリ消す処理
            }
            _filename = filename;
            if (string.IsNullOrEmpty(filename))
            {
                return;
            }
            scratchsound.addscratch(_filename,0);
            lblInfo.Text = scratchsound.GrainCount().ToString();

            //音声ファイル追加&インデックスを変更。既に追加済みならインデックスのみ変更
            _que = 0;
            pctRefresh();

        }

        private void frmScratch_Resize(object sender, EventArgs e)
        {
            pctRefresh();
        }

        private void pctWave_MouseDown(object sender, MouseEventArgs e)
        {
            _que = e.X * scratchsound.GrainCount() / pctWave.Image.Width;
            scratchsound.SetGrainIndex(_que);
            lblInfo.Text = _que.ToString() + "/" + scratchsound.GrainCount().ToString();
            pctRefresh();
        }

        private void frmScratch_FormClosing(object sender, FormClosingEventArgs e)
        {
            SoundDriver.Dispose();
        }

        bool inPlay = false;

        private void tmrStreaming_Tick(object sender, EventArgs e)
        {
            if (_filename != "")
            {
                if (inPlay)
                {
                    scratchsound.DoFrame(1, DateTime.Now);
                    pctRefresh();
                }
                else
                {
                    scratchsound.DoFrame(0, DateTime.Now);
                }
            }

        }

        private void pctRefresh()
        {
            if (_filename != "")
            {
                Bitmap bmp = scratchsound.ScratchBitmapWithLine(new Bitmap(pctWave.Width, pctWave.Height));
                for (int y = 0; y < bmp.Height - 1; y++)
                {
                    bmp.SetPixel(scratchsound.ScratchBitmapXposFromGrainIndex(_que), y, Color.Red);
                }
                pctWave.Image = (Image)bmp;
            }
        }

        /// <summary>
        /// 再生、停止の切り替え
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPlay_Click(object sender, EventArgs e)
        {
            inPlay = !inPlay;
        }

        /// <summary>
        /// Queポイントまで戻るQueのセットはマウスクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnQue_Click(object sender, EventArgs e)
        {
            scratchsound.SetGrainIndex(_que);
        }


    }
}
