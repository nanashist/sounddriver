using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio;
using NAudio.Wave;

namespace Sound
{

    public partial class FrameManager
    {

        public IWaveProvider OutputWaveProvider { get { return streamingbuffer.OutputWaveProvider; } }
        public StreamingBuffer streamingbuffer;

        private int samplerate;
        //毎フレームのRawdata(前フレームが１フレ以上のデータがあれば次フレはデータなし)
        private List<FrameRawData> framelist = new List<FrameRawData>();
        double buffermsec = 100;//50;

        public int FrameCount { get { return framelist.Count; } }

        /// <summary>
        /// Add済みのデータの再生終了予定(バッファ送信基準ではなくデータ追加基準)累積値
        /// </summary>
        private DateTimeEx DataEndTime;
        /// <summary>
        /// 最終追加分がループポイントをまたぐかどうか(Addごとに更新)
        /// </summary>
        public bool LoopPoint;

        private double realgrain;
        DateTimeEx initialtime;

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="now"></param>
        public void Init(DateTime now)
        {
            int mergesize;
            List<Int16> I16listL;
            List<Int16> I16listR;

            scratchWaveRaw.MakeSilentList(buffermsec, out mergesize, out I16listL, out I16listR);
            //framelist.Add(new FrameRawData(now.AddTicks((long)(-buffermsec * MyUtil.ticks2msec)), 1, false, 0, mergesize, I16listL, I16listR, 0, 0, 0));
            realgrain = 0;//基準点
            initialtime = new DateTimeEx(now);
        }
        //public bool ClearFuture(double speed,bool reverse)
        //{
        //    FrameRawData pre = framelist.Last();
        //    double rate = speed / pre.Speed;
        //    //速度が反転しているか、倍以上の速度差があって等速以上になるならバッファをクリアして続ける
        //    if (reverse != pre.Reverse){}
        //    else if(rate > 2 && Math.Abs(speed) >= 1){}
        //    else
        //    {
        //        //バッファクリアしないならここで抜ける
        //        //Debug.WriteLine("低速維持");
        //        return false;
        //    }
        //    if (pre.kesiteii <= 0)
        //    {
        //        return false;
        //    }
        //    pre.RawDataL.RemoveRange(pre.RawDataL.Count - pre.kesiteii, pre.kesiteii);
        //    pre.RawDataR.RemoveRange(pre.RawDataR.Count - pre.kesiteii, pre.kesiteii);
        //    return true;
        //}

        /// <summary>
        /// now以降のフレームデータを削除
        /// </summary>
        /// <param name="now"></param>
        /// <returns></returns>
        public bool ClearFuture(DateTime now)
        {
            List<FrameRawData> removelist = new List<FrameRawData>();
            foreach (FrameRawData f in framelist)
            {
                if (now.Ticks < f.MakeTime.Ticks)
                {
                    removelist.Add(f);
                }
            }
            if (removelist.Count == 0)
            {
                return false;
            }
            else
            {
                foreach (FrameRawData f in removelist)
                {
                    framelist.Remove(f);
                }
                return true;
            }
        }
        /// <summary>
        /// フレームデータを追加
        /// 基本1/60秒に寄せるが遅いとはみ出す。
        /// </summary>
        /// <param name="frame"></param>
        public void Add(FrameRawData frame)
        {
            //データなしなら作る必要なし
            if (frame.RawDataL.Count == 0) return;

            FrameRawData pre = Last();
            /*
            frame.konkaituika = frame.RawDataL.Count;
            if (pre.MergeSize == 0)
            {
                frame.kesiteii = 0;
                frame.konkaiamari = 0;
            }
            else
            {
                Int64 keikasample = (frame.MakeTime.Ticks - pre.MakeTime.Ticks) * samplerate / 10000000;
                frame.konkaiamari = (int)(pre.konkaiamari + pre.konkaituika - keikasample);
                if (frame.Speed >= 1)
                {
                    frame.kesiteii = 0;
                }
                else
                {
                    frame.kesiteii = (int)(frame.konkaituika - frame.MergeSize - (16.66 * samplerate / 1000));
                }
            }
            //
             * */
            //Ticks単位で今回フレームの再生時間数
            frame.DataTicks = (frame.RawDataL.Count - frame.MergeSize) * MyUtil.ticks2sec / samplerate;

            //再生開始時間
            frame.PlayTime = frame.MakeTime.AddMilliseconds(buffermsec);

            if (framelist.Count == 0)
            {
                DataEndTime = new DateTimeEx(frame.MakeTime.Ticks + frame.DataTicks);
            }
            else
            {
                DataEndTime = new DateTimeEx(DataEndTime.Ticks + frame.DataTicks);
            }
            frame.DataEndTime = DataEndTime;
            framelist.Add(frame);
            Test.Print(frame.DebugString());
            LoopPoint = false;
            if (frame.GrainStart != frame.GrainNext)
            {

                if (frame.Reverse)
                {
                    if (frame.GrainStart < frame.GrainNext)
                        LoopPoint = true;
                }
                else
                {
                    if (frame.GrainStart > frame.GrainNext)
                        LoopPoint = true;
                }
            }
        }
        /// <summary>
        /// framelistからいい感じのものをstreamingbufferに流し込んでいく
        /// </summary>
        public void Streaming()
        {
            FrameRawData nextframe = null;
            List<FrameRawData> removelist = new List<FrameRawData>();
            foreach (FrameRawData frame in framelist)
            {
                if (nextframe != null)
                {
                    nextframe.MergeFrameRawDataOld(frame);
                    nextframe = null;
                }
                if (frame.PlayTime <= DateTimeEx.Now)
                {
                    nextframe = frame;
                    removelist.Add(frame);
                }
            }
            foreach (FrameRawData f in removelist)
            {
                streamingbuffer.AddInt16Buffer(f.RawDataL, f.RawDataR);
                framelist.Remove(f);
            }
            streamingbuffer.Streaming();
        }

        //public void NextFrame(DateTime now)
        //{
        //    if (framelist.Count == 0) return; 
        //    FrameRawData pre = framelist.Last();
        //    Int64 keikasample = (now.Ticks - pre.MakeTime.Ticks) * samplerate / 10000000;
        //    pre.kesiteii -= (int)keikasample;
        //}

        /// <summary>
        /// 最後を取り出す
        /// </summary>
        /// <returns></returns>
        public FrameRawData Last()
        {
            if (framelist.Count == 0)
                return null;
            else
                return framelist.Last();
        }

        public void Clear()
        {
            framelist.Clear();
        }
        /// <summary>
        /// 前回フレーム情報を元に今回どのくらいつめるかを返す(これプラスマージ分つめる)
        /// </summary>
        /// <param name="now"></param>
        /// <returns></returns>
        public double GetNeedmsec(DateTime now)
        {
            double msec = 16.66;
            if (framelist.Count == 0)
            {
                this.Init(now);
                msec = 16.66;//空のときは１フレ
            }
            else
            {

                FrameRawData pre = framelist.Last();

                double amari = (double)(DataEndTime.Ticks - now.Ticks) / MyUtil.ticks2msec;
                if (amari > 16.66)
                {
                    msec = 0;//1フレ分以上まだ余ってるから今回は不要
                }
                else
                {
                    //amariが-になったらどうしようか。
                    msec = 16.66 - amari;//余剰分は引いて16msecに近づける
                }
            }

            return msec;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="waveformat"></param>
        public FrameManager(WaveFormat waveformat)
        {
            streamingbuffer = new StreamingBuffer(waveformat);
            samplerate = waveformat.SampleRate;
        }


    }
}
