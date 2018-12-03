using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using NAudio;
using NAudio.Wave;
using System.Drawing;
namespace Sound
{
    /// <summary>
    /// スクラッチファイルをリスト管理
    /// </summary>
    public partial class ScratchSound
    {
        public FrameManager framemanager;
        public IWaveProvider OutputWaveProvider { get { return framemanager.OutputWaveProvider; } }
        /// <summary>
        /// Rawデータ
        /// </summary>
        private List<scratchWaveRaw> ScratchWaveRawDataList;
        private Dictionary<string, short> dctScratchIndex;

        private int _mixersamplerate;

        private float bytepermsec;
        /// <summary>
        /// Rawデータのインデックス
        /// </summary>
        private int ScratchWaveIndex;

        #region "非再生"
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="waveformat">メインで使用しているWaveFormat。mp3読んだらIEEEフォーマットの44100SampleRateだった</param>
        public ScratchSound(WaveFormat waveformat)
        {
            _mixersamplerate = waveformat.SampleRate;
            bytepermsec = waveformat.SampleRate / 1000;//1msecで何サンプルか
            framemanager = new FrameManager(waveformat);
            ScratchWaveRawDataList = new List<scratchWaveRaw>();
            dctScratchIndex = new Dictionary<string, short>();
            ScratchWaveIndex = 0;
        }

        public ScratchSound(){
            WaveFormat waveformat = WaveFormatExtensible.CreateIeeeFloatWaveFormat(44100, 2);
            _mixersamplerate = waveformat.SampleRate;
            bytepermsec = waveformat.SampleRate / 1000;//1msecで何サンプルか
            framemanager = new FrameManager(waveformat);
            ScratchWaveRawDataList = new List<scratchWaveRaw>();
            dctScratchIndex = new Dictionary<string, short>();
            ScratchWaveIndex = 0;
        }
        /// <summary>
        /// スクラッチに使うWAVデータを追加&インデックスを変更。既に追加済みならインデックスのみ変更
        /// </summary>
        /// <param name="filename"></param>
        public void addscratch(string filename,int grainindex)
        {
            if (!dctScratchIndex.ContainsKey(filename))
            {
                scratchWaveRaw sw;
                dctScratchIndex.Add(filename, (short)ScratchWaveRawDataList.Count);
                if (filename == "sine")
                {
                    sw = new scratchWaveRaw(_mixersamplerate);
                    sw.MakeSineData();
                }
                else
                {
                    sw = new scratchWaveRaw(filename, grainindex, _mixersamplerate);
                }
                ScratchWaveRawDataList.Add(sw);
            }
            ScratchWaveIndex = dctScratchIndex[filename];
        }
        public void Clear()
        {
        }

        /// <summary>
        /// 再生位置をスタートに戻す
        /// </summary>
        public void reset()
        {
            ScratchWaveRawDataList[ScratchWaveIndex].GrainIndexReset();
        }

        /// <summary>
        /// アクティブスクラッチの現在のグレインのindexを取得
        /// </summary>
        /// <returns></returns>
        public int GrainIndex()
        {
            return ScratchWaveRawDataList[ScratchWaveIndex].GrainIndex();
        }
        /// <summary>
        /// アクティブスクラッチのMaxグレイン数を取得
        /// </summary>
        /// <returns></returns>
        public int GrainCount()
        {
            if (ScratchWaveRawDataList.Count > ScratchWaveIndex)
            {
                return ScratchWaveRawDataList[ScratchWaveIndex].MaxGrainIndex();
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// グレインのindexをセット
        /// </summary>
        /// <param name="index"></param>
        public void SetGrainIndex(int index)
        {
            ScratchWaveRawDataList[ScratchWaveIndex].GrainIndexSet(index);
        }

        #endregion

        public int FrameCount { get { return framemanager.FrameCount; } }

        /// <summary>
        /// 前回呼んだときからの差分時間分のデータをframemanager.framelistに追加
        /// バッファが空なら16.66msec分追加
        /// </summary>
        /// <param name="deltaSpeed">今回追加する分の速度</param>
        /// <param name="now"></param>
        /// <returns>音量の平均</returns>
        public Int16 MakeRawDataDeltaTime(double deltaSpeed, DateTime now)
        {
            //前回呼んだときからの差分時間分のデータをframelistに追加
            double speed = deltaSpeed;
            double msec;
            bool reverse = false;

            if (deltaSpeed < 0)
            {
                speed = -deltaSpeed;
                reverse = true;
            }

            int mergesize;
            List<Int16> I16listL;
            List<Int16> I16listR;
            int grainStart = 0;
            int grainEnd = 0;
            double grainCount = 0;

            msec = framemanager.GetNeedmsec(now);//何msecつめようか。
            //msecは0以下にならないのでとりえあずこのまま。0の時廃棄する処理を追加するとより良いかも。

            //作成予定のmsec(グレイン単位で作成するので実際はこれより大きくなる)
            if (ScratchWaveRawDataList.Count > ScratchWaveIndex && speed > 0.1)
            {
                ScratchWaveRawDataList[ScratchWaveIndex].MakeSampleList(speed, msec, reverse, out mergesize, out I16listL, out I16listR, out grainStart, out grainEnd, out grainCount);
            }
            else
            {
                //WaveIndexが不正、速度一定以下は無音にしてしまう。
                scratchWaveRaw.MakeSilentList(msec, out mergesize, out I16listL, out I16listR);
            }
            //生データでの時間から理論時間を引いて余剰をrestとして保存しておく
            double rest = scratchWaveRaw.GetSample2msec(I16listL.Count - mergesize) - msec;//カウント
            framemanager.Add(new FrameRawData(now, speed, reverse, rest, mergesize, I16listL, I16listR, grainStart, grainEnd, grainCount));

            if (I16listL.Count - mergesize > 0)
            {
                long sum = 0;
                foreach (Int16 i in I16listL)
                    sum += Math.Abs(i / 256);
                return (short)(sum / (I16listL.Count - mergesize));
            }
            else
            {
                return 0;
            }
        }
        
        /// <summary>
        /// 毎フレーム呼んでスクラッチ音声の流し込みとバッファストリーミング
        /// </summary>
        /// <param name="deltaSpeed"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        public Int16 DoFrame(double deltaSpeed, DateTime now)
        {
            Int16 rtn = MakeRawDataDeltaTime(deltaSpeed, now);
            framemanager.Streaming();
            return rtn;
        }

        #region "画像"
        /// <summary>
        /// imageに収まるサイズで画像を作成
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public Bitmap ScratchBitmap(Image image)
        {
            if (ScratchWaveRawDataList.Count > ScratchWaveIndex)
            {
                Bitmap rtn = ScratchWaveRawDataList[ScratchWaveIndex].scratchBmp(image);
                return rtn;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 毎秒何ドット、高さを指定して画像を作成
        /// </summary>
        /// <param name="dotpersec"></param>
        /// <param name="band"></param>
        /// <returns></returns>
        public Bitmap ScratchBitmap(int dotpersec, int band)
        {
            if (ScratchWaveRawDataList.Count > ScratchWaveIndex)
            {
                Bitmap rtn = ScratchWaveRawDataList[ScratchWaveIndex].scratchBmp(dotpersec, band);
                return rtn;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 現在の再生位置に線を引いたBitmapを返す
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public Bitmap ScratchBitmapWithLine(Image image)
        {
            if (ScratchWaveRawDataList.Count > ScratchWaveIndex)
            {
                Bitmap rtn = ScratchWaveRawDataList[ScratchWaveIndex].scratchBmp(image);
                int x = ScratchWaveRawDataList[ScratchWaveIndex].scratchBmp_Xpos(GrainIndex());
                if (x >= rtn.Width) x = rtn.Width - 1;
                for (int y = 0; y < rtn.Height - 1; y++)
                {
                    rtn.SetPixel(x, y, Color.White);
                }
                return rtn;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 現在の再生位置に線を引いたBitmapを返す
        /// </summary>
        /// <param name="dotpersec"></param>
        /// <param name="band"></param>
        /// <returns></returns>
        public Bitmap ScratchBitmapWithLine(int dotpersec, int band)
        {
            if (ScratchWaveRawDataList.Count > ScratchWaveIndex)
            {
                Bitmap bmp = ScratchWaveRawDataList[ScratchWaveIndex].scratchBmpCentering(dotpersec, band, (double)GrainIndex() / GrainCount());
                return bmp;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// グレインインデックス指定でX座標を返す
        /// </summary>
        /// <param name="gindex"></param>
        /// <returns></returns>
        public int ScratchBitmapXposFromGrainIndex(int gindex)
        {
            if (ScratchWaveRawDataList.Count > ScratchWaveIndex)
                return ScratchWaveRawDataList[ScratchWaveIndex].scratchBmp_Xpos(gindex);
            else
                return 0;
        }

        #endregion
    }

}