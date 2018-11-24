using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NAudio;
using NAudio.Wave;
using System.Diagnostics;
namespace Sound
{
    partial class FrameManager
    {

        
        /// <summary>
        /// 再生時刻(PlayTime)を過ぎたものを
        /// FrameManagerからストリーミングバッファにデータを流す
        /// </summary>
        public void TestStreaming()
        {
            Test.Print("TestStreaming  in");
            List<FrameRawData> removelist = new List<FrameRawData>();
            foreach (FrameRawData f in framelist)
            {
                if (f.PlayTime < DateTimeEx.Now)
                {
                    removelist.Add(f);
                    break;
                }
            }

            if (removelist.Count > 0)
            {
                foreach (FrameRawData f in removelist)
                {
                    framelist.Remove(f);
                }
                FrameRawData merged = FrameRawData.MergeFrameList(removelist);

                if (framelist.Count > 0)
                {
                    Test.Print("TestStreaming  いい感じにストリーミング");
                    List<short> rawL, rawR;
                    merged.CutBackMergeRawData(out rawL,out rawR);
                    framelist.First().MergeFront(rawL, rawR);
                    streamingbuffer.AddInt16Buffer(merged.RawDataL, merged.RawDataR);
                }
                else
                {
                    Test.Print("TestStreaming  バッファ空（framelistなくなった)");
                    streamingbuffer.AddInt16Buffer(merged.RawDataL, merged.RawDataR);
                }
            }
            //バッファストリーミング処理
            streamingbuffer.Streaming();

        }

        //生データ一括で送信するテスト
        public void TestAllPlay(PictureBox pct)
        {
            //とりあえず元データを傷つけ無いようにクローン
            if (framelist.Count < 1) return;
            List<FrameRawData> flist = new List<FrameRawData>();
            foreach (FrameRawData f in framelist)
            {
                flist.Add(f.Clone());
            }
            flist.Remove(flist.First());
            FrameRawData first;
            first = flist.First();
            flist.Remove(first);
            foreach (FrameRawData f in flist)
            {
                first.JointFrameRawData(f);
                //first.RawDataL.AddRange(f.RawDataL);
                //first.RawDataR.AddRange(f.RawDataR);
            }
            streamingbuffer.AddInt16Buffer(first.RawDataL, first.RawDataR);

            DateTime now;
            DateTime delta = DateTime.Now;
            while (true)
            {
                now = DateTime.Now;

                Application.DoEvents();
                if (delta.Ticks < now.Ticks)
                {
                    delta = delta.AddMilliseconds(16.66);
                }
                if (streamingbuffer.GetBufferCount() == 0)
                {
                    break;
                }
                streamingbuffer.Streaming();
            }
        }
    }
}