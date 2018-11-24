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
        public void addscratch(string filename)
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
                    sw = new scratchWaveRaw(filename, _mixersamplerate);
                }
                ScratchWaveRawDataList.Add(sw);
            }
            ScratchWaveIndex = dctScratchIndex[filename];
        }

        /// <summary>
        /// 再生位置をスタートに戻す
        /// </summary>
        public void reset()
        {
            ScratchWaveRawDataList[ScratchWaveIndex].GrainIndexReset();
        }

        /// <summary>
        /// グレインのindexを取得
        /// </summary>
        /// <returns></returns>
        public int GrainIndex()
        {
            return ScratchWaveRawDataList[ScratchWaveIndex].GrainIndex();
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
        public void MakeRawDataDeltaTime(double deltaSpeed, DateTime now)
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
        }



        #region "画像"
        public Bitmap ScratchBitmap(int dotpersec, int band)
        {
            Bitmap rtn = ScratchWaveRawDataList[ScratchWaveIndex].scratchBmp(dotpersec, band);
            return rtn;
        }
        #endregion
    }

}