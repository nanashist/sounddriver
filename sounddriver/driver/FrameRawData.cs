using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
namespace Sound
{

    [DebuggerDisplay("{MakeTime.Millis}～{DataEndTime.Millis} 再生{PlayTime.Millis} グレイン{GrainStart}～{GrainNext}")]
    public class FrameRawData
    {
        #region"データ作成後計算するやつ"
        /// <summary>
        /// 再生開始時間(バッファリングに送信する時刻)
        /// </summary>
        public DateTimeEx PlayTime;
        /// <summary>
        /// データ長(DatetimeのTicks単位) (1 ミリ秒 = 10000 タイマ刻み) (マージ部分含まず
        /// </summary>
        public long DataTicks;
        /// <summary>
        /// デバッグ用再生終了時間(MakeTime基点でDataTicksの再生終了時刻
        /// </summary>
        public DateTimeEx DataEndTime;
        #endregion

        #region "プロパティ(嘘。全部変数直接公開してるだけ)


        /// <summary>
        /// 作成時間(初回処理時間であり、バッファリングの開始時刻の目安)
        /// </summary>
        public DateTimeEx MakeTime;

        /// <summary>
        /// 今回処理で出たあまり(msec)１フレームにグレイン単位でつめるのであまる
        /// </summary>
        public double Restmsec;
        /// <summary>
        /// 前フレームと融合するカウント
        /// </summary>
        public int MergeSize;
        /// <summary>
        /// 開始グレイン位置
        /// </summary>
        public int GrainStart;
        /// <summary>
        /// 終了グレイン位置
        /// </summary>
        public int GrainNext;
        /// <summary>
        /// 計算上の理論グレイン値（今回フレーム分をグレイン換算(実数))
        /// </summary>
        public double RealGrainCount;
        /// <summary>
        /// 作成時の速度
        /// </summary>
        public double Speed;
        /// <summary>
        /// 作成時の再生方向
        /// </summary>
        public bool Reverse;
        /// <summary>
        /// 今回フレームの生データL
        /// </summary>
        public List<Int16> RawDataL;
        /// <summary>
        /// 今回フレームの生データR
        /// </summary>
        public List<Int16> RawDataR;

        #endregion
        /// <summary>
        /// フレームデータの作成
        /// </summary>
        /// <param name="maketime">作成時間</param>
        /// <param name="speed">再生速度（参考値）</param>
        /// <param name="reverse">逆再生（参考値）</param>
        /// <param name="restmsec"></param>
        /// <param name="mergesize">マージするサンプル数</param>
        /// <param name="rawdatal">Rawデータ</param>
        /// <param name="rawdatar">Rawデータ</param>
        /// <param name="grainstart">開始グレイン値（参考値）</param>
        /// <param name="grainnext">次回グレイン値（参考値）</param>
        /// <param name="graincount">今回作成数（参考値）</param>
        public FrameRawData(DateTime maketime, double speed, bool reverse, double restmsec, int mergesize, List<Int16> rawdatal, List<Int16> rawdatar, int grainstart, int grainnext, double graincount)
        {
            SetData(new DateTimeEx(maketime), speed, reverse, restmsec, mergesize, rawdatal, rawdatar, grainstart, grainnext, graincount);
        }

        public FrameRawData(DateTimeEx maketime, double speed, bool reverse, double restmsec, int mergesize, List<Int16> rawdatal, List<Int16> rawdatar, int grainstart, int grainnext, double graincount)
        {
            SetData(maketime, speed, reverse, restmsec, mergesize, rawdatal, rawdatar, grainstart, grainnext, graincount);
        }

        public void SetData(DateTimeEx maketime, double speed, bool reverse, double rest, int mergesize, List<Int16> rawdatal, List<Int16> rawdatar, int grainstart, int grainnext, double graincount)
        {
            MakeTime = maketime;
            //LastTime = maketime;
            Speed = speed;
            Reverse = reverse;
            Restmsec = rest;
            MergeSize = mergesize;
            RawDataL = rawdatal;
            RawDataR = rawdatar;
            GrainStart = grainstart;
            GrainNext = grainnext;
            RealGrainCount = graincount;
        }

        public FrameRawData Clone()
        {
            FrameRawData f = new FrameRawData(MakeTime, Speed, Reverse, Restmsec, MergeSize, new List<short>(), new List<short>(), GrainStart, GrainNext, RealGrainCount);
            f.RawDataL.AddRange(RawDataL);
            f.RawDataR.AddRange(RawDataR);
            return f;
        }

        /// <summary>
        /// このインスタンスに次のFrameのマージ部分だけ結合する
        /// </summary>
        /// <param name="newerdata"></param>
        public void MergeFrameRawDataOld(FrameRawData newerdata)
        {

            if (newerdata.MergeSize > 0)
            {
                int mergesize = newerdata.MergeSize;
                if (RawDataL.Count < mergesize)
                {
                    mergesize = RawDataL.Count;
                }
                if (newerdata.RawDataL.Count < mergesize)
                {
                    mergesize = newerdata.RawDataL.Count;
                }
                if (mergesize == newerdata.MergeSize)
                {
                    Debug.WriteLine("ちゃんとマージ");
                }
                else
                {
                    Debug.WriteLine("中途半端マージ");
                }
                List<Int16> mergeL = newerdata.RawDataL.GetRange(0, mergesize);
                List<Int16> mergeR = newerdata.RawDataR.GetRange(0, mergesize);
                newerdata.RawDataL.RemoveRange(0, mergesize);
                newerdata.RawDataR.RemoveRange(0, mergesize);
                for (int i = 0; i < mergesize; i++)
                {
                    RawDataL[RawDataL.Count - mergesize + i] = StreamingBuffer.AddInt16(RawDataL[RawDataL.Count - mergesize + i], mergeL[i]);
                    RawDataR[RawDataR.Count - mergesize + i] = StreamingBuffer.AddInt16(RawDataR[RawDataR.Count - mergesize + i], mergeR[i]);
                }
            }
        }
        /// <summary>
        /// 新しいフレームに直前フレームのマージ部分を合体する
        /// </summary>
        /// <param name="predata"></param>
        public void MergeFrameRawDataNew(FrameRawData predata)
        {

            if (MergeSize > 0)
            {
                int mergesize = MergeSize;
                if (RawDataL.Count < mergesize)
                {
                    mergesize = RawDataL.Count;
                }
                if (predata.RawDataL.Count < mergesize)
                {
                    mergesize = predata.RawDataL.Count;
                }
                if (mergesize == predata.MergeSize)
                {
                    Debug.WriteLine("ちゃんとマージ");
                }
                else
                {
                    Debug.WriteLine("中途半端マージ");
                }
                List<Int16> mergeL = predata.RawDataL.GetRange(predata.RawDataL.Count - mergesize, mergesize);
                List<Int16> mergeR = predata.RawDataR.GetRange(predata.RawDataR.Count - mergesize, mergesize);
                predata.RawDataL.RemoveRange(predata.RawDataL.Count - mergesize, mergesize);
                predata.RawDataR.RemoveRange(predata.RawDataR.Count - mergesize, mergesize);
                for (int i = 0; i < mergesize; i++)
                {
                    RawDataL[i] = StreamingBuffer.AddInt16(RawDataL[i], mergeL[i]);
                    RawDataR[i] = StreamingBuffer.AddInt16(RawDataR[i], mergeR[i]);
                }
            }
        }

        /// <summary>
        /// このインスタンスにnewerdataのRawdataを合体させる
        /// </summary>
        /// <param name="newerdata"></param>
        public void JointFrameRawData(FrameRawData newerdata)
        {
            MergeFrameRawDataOld(newerdata);
            RawDataL.AddRange(newerdata.RawDataL);
            RawDataR.AddRange(newerdata.RawDataR);
            MergeSize = newerdata.MergeSize;
        }

        public static FrameRawData MergeFrameList(List<FrameRawData> mergelist)
        {
            if (mergelist.Count == 0)
            {
                return null;
            }
            else if (mergelist.Count == 1)
            {
                return mergelist[0].Clone();
            }
            else
            {
                List<FrameRawData> flist = new List<FrameRawData>();
                FrameRawData first = mergelist.First();
                mergelist.Remove(first);
                foreach (FrameRawData f in mergelist)
                {
                    first.JointFrameRawData(f.Clone());
                }
                return first;
            }
        }

        /// <summary>
        /// 後ろのマージ部分を切り取る
        /// </summary>
        /// <param name="mergelistL"></param>
        /// <param name="mergelistR"></param>
        public void CutBackMergeRawData(out List<Int16> mergelistL, out List<Int16> mergelistR)
        {
            mergelistL=new List<short>();
            mergelistR=new List<short>();
            if (RawDataL.Count < MergeSize)
            {
                mergelistL.AddRange(RawDataL.GetRange(0, RawDataL.Count));
                mergelistR.AddRange(RawDataR.GetRange(0, RawDataR.Count));
                RawDataL = new List<short>();
                RawDataR = new List<short>();
            }
            else
            {
                mergelistL.AddRange(RawDataL.GetRange(RawDataL.Count - MergeSize, MergeSize));
                mergelistR.AddRange(RawDataR.GetRange(RawDataR.Count - MergeSize, MergeSize));
                RawDataL.RemoveRange(RawDataL.Count - MergeSize, MergeSize);
                RawDataR.RemoveRange(RawDataR.Count - MergeSize, MergeSize);
            }
        }

        /// <summary>
        /// 前からmergelistをマージする
        /// </summary>
        /// <param name="mergelistL"></param>
        /// <param name="mergelistR"></param>
        public void MergeFront(List<Int16> mergelistL, List<Int16> mergelistR)
        {
            int mergesize = mergelistL.Count;
            for (int i = 0; i < mergesize; i++)
            {
                RawDataL[i] = StreamingBuffer.AddInt16(RawDataL[i], mergelistL[i]);
                RawDataR[i] = StreamingBuffer.AddInt16(RawDataR[i], mergelistR[i]);
            }
        }

        public string DebugString()
        {
            string s;
            s = MakeTime.Millis.ToString() + "～" + DataEndTime.Millis.ToString() +"  再生" + PlayTime.Millis.ToString() + " グレイン" + GrainStart.ToString() + "～" + GrainNext.ToString();
            return s;
        }
    }
}