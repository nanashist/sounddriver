using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Sound
{
    /// <summary>
    /// スクラッチに使うWAVE生データの配列の画像作成用分割クラス
    /// </summary>
    partial class scratchWaveRaw
    {
        #region "Bitmap画像関連"

        Bitmap _bmpx2;
        int _bmpwidth;
        /// <summary>
        /// 波形Bitmap作成
        /// </summary>
        /// <param name="dotpersec">秒間何ドットか（横幅）</param>
        /// <param name="band">高さ</param>
        /// <returns></returns>
        public Bitmap scratchBmp(int dotpersec, int band)
        {
            int width = (ResampleI16RawDataL.Count() / _mixersamplerate) * dotpersec;
            if (_bmpwidth != width && _bmpx2 != null)
            {
                return _bmpx2.Clone(new Rectangle(0, 0, _bmpx2.Width / 2, _bmpx2.Height), _bmpx2.PixelFormat);
            }
            else
            {
                _bmpwidth = width;
                int height = band;
                int center = height / 2;
                return DrawBitmap(width, height, center);

            }

        }



        /// <summary>
        /// 波形Bitmap作成(貼り付けサイズいっぱいに作成
        /// </summary>
        /// <param name="destbmp"></param>
        /// <returns></returns>
        public Bitmap scratchBmp(Image destbmp)
        {
            int width = destbmp.Width;// (ResampleI16RawDataL.Count() / _mixersamplerate) * dotpersec;
            double dotpersec = (double)(width * _mixersamplerate / ResampleI16RawDataL.Count());

            if (_bmpwidth != destbmp.Width && _bmpx2 != null)
            {
                return _bmpx2.Clone(new Rectangle(0, 0, _bmpx2.Width / 2, _bmpx2.Height), _bmpx2.PixelFormat);
            }
            else
            {
                int height = destbmp.Height;
                int center = destbmp.Height / 2;
                _bmpwidth = destbmp.Width;
                return DrawBitmap(destbmp.Width, height, center);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="width">WAVEの幅。画像の幅を指定すると曲全体が1画像に収まる</param>
        /// <param name="drawheight">描画の高さ。画像の高さを指定するとマックス音量で両端に到達</param>
        /// <param name="center">画像の中心を渡す必要あり。描画の高さにすると上にずれる</param>
        /// <returns></returns>
        private Bitmap DrawBitmap(int width, int drawheight, int center)
        {
            Bitmap bmp = new Bitmap(width, drawheight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            double keisuu = (double)drawheight / 32767;
            for (int x = 0; x < width - 1; x++)
            {
                long i1 = (long)x * ResampleI16RawDataL.Count() / width;
                long i2 = (long)(x + 1) * ResampleI16RawDataL.Count() / width;
                long plussum = 0;
                long minussum = 0;
                for (long n = i1; n < i2; n++)
                {
                    if (ResampleI16RawDataL[n] > 0)
                    {
                        plussum += ResampleI16RawDataL[n];
                    }
                    else
                    {
                        minussum += ResampleI16RawDataL[n];
                    }
                }
                int start = (int)(center - keisuu * plussum / (i2 - i1));
                int end = (int)(center - keisuu * minussum / (i2 - i1));
                for (int y = start; y <= end; y++)
                {
                    bmp.SetPixel(x, y, Color.FromArgb(0, 255, 0));
                }
            }
            _bmpx2 = new Bitmap(bmp.Width * 2, bmp.Height, bmp.PixelFormat);
            Graphics g;
            g = Graphics.FromImage(_bmpx2);
            g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
            g.DrawImage(bmp, bmp.Width, 0, bmp.Width, bmp.Height);
            g.Dispose();
            return bmp;
        }

        public Bitmap scratchBmpWithLine(int dotpersec, int band, double xper)
        {
            Bitmap bmp = new Bitmap(scratchBmp(dotpersec, band));
            int x = (int)(xper * bmp.Width);
            if (x < bmp.Width)
            {
                for (int y = 0; y < bmp.Height - 1; y++)
                {
                    bmp.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                }
            }
            return bmp;
        }
        /// <summary>
        /// 不要かも。
        /// </summary>
        /// <param name="dotpersec"></param>
        /// <param name="band"></param>
        /// <param name="xper"></param>
        /// <returns></returns>
        public Bitmap scratchBmpCentering(int dotpersec, int band, double xper)
        {
            if (_bmpx2 == null)
                scratchBmp(dotpersec, band);
            Bitmap bmp = new Bitmap(_bmpx2.Width / 2, _bmpx2.Height, _bmpx2.PixelFormat);
            Graphics g;
            g = Graphics.FromImage(bmp);
            int x;
            if (xper + 0.5 > 1)
                x = (int)((xper - 0.5) * bmp.Width);
            else
                x = (int)((xper + 0.5) * bmp.Width);

            if (x < bmp.Width)
            {
                g.DrawImage(_bmpx2, new Rectangle(0, 0, bmp.Width, bmp.Height), new Rectangle(x, 0, bmp.Width, bmp.Height), GraphicsUnit.Pixel);
            }
            x = bmp.Width / 2;
            for (int y = 0; y < bmp.Height - 1; y++)
            {
                bmp.SetPixel(x, y, Color.FromArgb(255, 255, 255));
            }

            g.Dispose();
            return bmp;
        }

        /// <summary>
        /// grainIndexから再生位置のX座標を取得
        /// </summary>
        /// <param name="grainCount"></param>
        /// <returns></returns>
        public int scratchBmp_Xpos(int graincount)
        {
            return (int)(graincount * _bmpwidth / MaxGrainIndex());
        }

        #endregion
    }
}
