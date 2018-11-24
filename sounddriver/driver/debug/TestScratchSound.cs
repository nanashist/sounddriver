using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Sound
{

    partial class ScratchSound
    {
        public bool stopflg = false;
        public double speed;

        /// <summary>
        /// 1/60秒毎にグレインを継ぎ足し
        /// </summary>
        /// <param name="sp"></param>
        public void TestOneFrameMake(double sp)
        {
            Test.Print("TestOneFrameMake  in");
            //今回フレーム分framemanagerにWaveデータ追加
            MakeRawDataDeltaTime(sp , DateTime.Now);
        }

        /// <summary>
        /// 1/60秒毎に呼ぶ。ストリーミング処理
        /// 実践テスト、ストリーミング
        /// </summary>
        public void TestStreaming()
        {
            //framemanager.framelistをstreamingbufferに流す＆streamingbufferのストリーミング処理
            framemanager.TestStreaming();
        }


        /// <summary>
        /// 実践テスト
        /// </summary>
        public void TestMake()
        {
            Test.Print("TestMake  in");
            DateTime now = DateTime.Now;
            DateTime delta = DateTime.Now;
            DateTimeEx.Init();
            SetGrainIndex(0);

            //無限ループ
            while (true)
            {
                now = DateTime.Now;
                Application.DoEvents();
                if (now >= delta)
                {
                    delta = delta.AddMilliseconds(16.66);
                    //今回フレーム分framemanagerにWaveデータ追加
                    MakeRawDataDeltaTime(speed / 10, now);

                    framemanager.TestStreaming();

                    //バッファストリーミング処理
                    framemanager.streamingbuffer.Streaming();

                }
                if (stopflg)
                    break;
            }
        }


        //指定速度で1周期再生テスト
        public void TestMake2(double speed)
        {
            DateTime now = DateTime.Now;
            DateTime delta = DateTime.Now;
            DateTimeEx.Init();
            SetGrainIndex(0);
            int cnt = 1000;
            for (int i = 0; i < 1000; i++)
            {
                MakeRawDataDeltaTime(speed / 10, now);
                now = now.AddMilliseconds(16.66);
                if (framemanager.LoopPoint)
                {
                    cnt = i;
                    break;
                }
            }

            for (int i = 0; i < cnt; i++)
            {
                MakeRawDataDeltaTime((double)(-speed / 10), now);
                now = now.AddMilliseconds(16.66);
            }
        }
    }
}