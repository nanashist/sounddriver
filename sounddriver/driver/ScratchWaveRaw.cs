﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio;
using NAudio.Wave;

namespace Sound
{
    /// <summary>
    /// スクラッチに使うWAVE生データの配列、グレイン分割したグレイン
    /// </summary>
    public partial class scratchWaveRaw
    {
        const int _grainmsec = 25;//グレインの半分のmsec
                
        string _filename;
        int _grainIndexdefault;
        /// <summary>
        /// ミキサーのサンプリングレート
        /// </summary>
        static int _mixersamplerate;

        //int _pos;//現在位置
        //int _length;//サンプル数
        /// <summary>
        /// _mixersamplerateで-32767～32767の16bitステレオの生データ
        /// </summary>
        Int16[] ResampleI16RawDataL;
        /// <summary>
        /// _mixersamplerateで-32767～32767の16bitステレオの生データ
        /// </summary>
        Int16[] ResampleI16RawDataR;

        /// <summary>
        /// grainの計算がややこしくなる可能性があるのでグレイン分割の時点でリサンプルして44100Hz固定にする
        /// </summary>
        List<Int16[]> _grainListL;
        List<Int16[]> _grainListR;

        int _grainIndex;
        /// <summary>
        /// グレインの実サイズ(50msecでのサンプル数)
        /// </summary>
        int _grainSize;//50msecのサンプル数(1/20sec)
        int _grainHalfSize;

        public int GrainHalfSize { get { return _grainHalfSize; } }

        /// <summary>
        /// 1グレインのmsec(50msecで1周期だが半分はクロスフェードするので実効部分25msecを返す)
        /// </summary>
        public static int Grainmsec { get { return _grainmsec; } }

        public static double GetSample2msec(int samples)
        {
            return ((double)samples * 1000 / _mixersamplerate);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="filename"></param>
        public scratchWaveRaw(string filename, int grainindexdefault, int mixersamplerate)
        {
            int originalsamplerate;//サンプリングレート
            _filename = filename;
            _grainIndexdefault = grainindexdefault;
            //ファイルを読み込んで生データにする
            {
                float[] RawData;//RAWデータ
                float[] RawDataL;
                float[] RawDataR;
                AudioFileReader audioreader = new AudioFileReader(filename);
                originalsamplerate = audioreader.WaveFormat.SampleRate;
                //_pos = 0;
                _grainIndex = 0;
                int len = (int)audioreader.Length / audioreader.WaveFormat.BlockAlign;
                RawData = new float[len];
                audioreader.Read(RawData, 0, len);
                if (audioreader.WaveFormat.Channels == 1)
                {
                    RawDataL = new float[RawData.Length];
                    RawDataR = new float[RawData.Length];
                    for (int i = 0; i < RawData.Length; i++)
                    {
                        RawDataL[i] = RawData[i];
                        RawDataR[i] = RawData[i];
                    }
                }
                else
                {
                    RawDataL = new float[RawData.Length / 2];
                    RawDataR = new float[RawData.Length / 2];
                    for (int i = 0; i < RawDataL.Length; i++)
                    {
                        RawDataL[i] = RawData[i * 2];
                        RawDataR[i] = RawData[i * 2 + 1];
                    }
                }

                //リサンプリング(ニアレストネイバー的に)
                _mixersamplerate = mixersamplerate;
                ResampleI16RawDataL = new Int16[(int)((double)RawDataL.Length * _mixersamplerate / originalsamplerate)];
                ResampleI16RawDataR = new Int16[(int)((double)RawDataR.Length * _mixersamplerate / originalsamplerate)];
                for (int i = 0; i < ResampleI16RawDataL.Length; i++)
                {
                    Int32 index = (Int32)((double)i * RawDataL.Length / ResampleI16RawDataL.Length);
                    ResampleI16RawDataL[i] = (Int16)(RawDataL[index] * 32767);
                    ResampleI16RawDataR[i] = (Int16)(RawDataR[index] * 32767);
                }
            }
            ResampleRawData2GrainList();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="mixersamplerate"></param>
        public scratchWaveRaw(int mixersamplerate)
        {
            _mixersamplerate = mixersamplerate;
            _grainIndexdefault = 0;
        }

        /// <summary>
        /// 440HzSin波を作成
        /// </summary>
        public void MakeSineData()
        {
            ResampleI16RawDataL = new Int16[_mixersamplerate * 2];
            ResampleI16RawDataR = new Int16[_mixersamplerate * 2];
            double onecycle = _mixersamplerate / 440;//440Hz
            double sum = 0;
            for (int i = 0; i < ResampleI16RawDataL.Length; i++)
            {
                if (i >= sum)
                    sum += onecycle;
                if ((sum - i) > onecycle / 2)
                {
                    ResampleI16RawDataL[i] = 32767;
                    ResampleI16RawDataR[i] = 32767;
                }
                else
                {
                    ResampleI16RawDataL[i] = -32767;
                    ResampleI16RawDataR[i] = -32767;
                }
            }
            ResampleRawData2GrainList();
        }


        /// <summary>
        /// ResampleI16RawDataをグレインリストに変換
        /// </summary>
        public void ResampleRawData2GrainList()
        {
            //グラニュラー変換でグレインのリスト作成
            {
                _grainHalfSize = _mixersamplerate * _grainmsec / 1000;//(int)(audioreader.WaveFormat.SampleRate * 50 / 1000);
                _grainSize = _grainHalfSize * 2;//cosを掛けるから有効なのは中央の半分程度なので100msecのサイズのバッファ
                {
                    //左チャンネル
                    _grainListL = new List<Int16[]>();
                    int count = (int)(ResampleI16RawDataL.Length / _grainHalfSize);//50msecごとに刻んで100msecのバッファリストを作成していく
                    count -= 1;
                    for (int i = 0; i < count; i++)
                    {
                        Int16[] temp = new Int16[_grainSize];
                        for (int n = 0; n < _grainSize; n++)
                        {
                            temp[n] = (Int16)(((1 - Math.Cos(Math.PI * 2 * n / _grainSize)) / 2 * ResampleI16RawDataL[i * _grainHalfSize + n]));
                        }
                        _grainListL.Add(temp);
                    }
                }
                {
                    //右チャンネル
                    _grainListR = new List<Int16[]>();
                    int count = (int)(ResampleI16RawDataR.Length / _grainHalfSize);//50msecごとに刻んで100msecのバッファリストを作成していく
                    count -= 1;
                    for (int i = 0; i < count; i++)
                    {
                        Int16[] temp = new Int16[_grainSize];
                        for (int n = 0; n < _grainSize; n++)
                        {
                            temp[n] = (Int16)(((1 - Math.Cos(Math.PI * 2 * n / _grainSize)) / 2 * ResampleI16RawDataR[i * _grainHalfSize + n]));
                        }
                        _grainListR.Add(temp);
                    }
                }
            }
        }

        #region "公開関数"
        /// <summary>
        /// 現在のグレインインデックスの取得
        /// </summary>
        /// <returns></returns>
        public int GrainIndex()
        {
            return _grainIndex;
        }

        public int MaxGrainIndex()
        {
            return _grainListL.Count - 1;
        }
        /// <summary>
        /// グレインインデックスの初期化
        /// </summary>
        public void GrainIndexReset()
        {
            _grainIndex = _grainIndexdefault;
        }
        /// <summary>
        /// グレインインデックスの指定
        /// </summary>
        /// <param name="index"></param>
        public void GrainIndexSet(int index)
        {
            _grainIndex = index;
        }

        public static void MakeSilentList(double msec, out int mergesize, out List<Int16> outRawDataL, out List<Int16> outRawDataR)
        {
            mergesize = 0;
            outRawDataL = new List<Int16>();
            outRawDataR = new List<Int16>();
            int listcount = msec2SampleCount(msec);
            for (int i = 0; i < listcount; i++)
            {
                outRawDataL.Add(0);
                outRawDataR.Add(0);
            }
        }

        public static int msec2SampleCount(double msec)
        {
            return (int)(msec * _mixersamplerate / 1000);
        }

        /// <summary>
        /// 速度、正逆方向を指定して指定msec分のWAVrawデータのリストを作成する
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="msec"></param>
        /// <param name="reverse"></param>
        /// <param name="mergesize"></param>
        /// <param name="outRawDataL"></param>
        /// <param name="outRawDataR"></param>
        public void MakeSampleList(double speed, double msec, bool reverse, out int mergesize, out List<Int16> outRawDataL, out List<Int16> outRawDataR, out int outGrainStart, out int outGrainNext, out double outGrainCount)
        {
            outRawDataL = new List<Int16>();
            outRawDataR = new List<Int16>();
            outGrainStart = 0;
            outGrainNext = 0;
            outGrainCount = 0;

            if (Math.Abs(speed) < 0.01)
            {
                mergesize = 0;
                return;
            }


            int grainHalfSamples = (int)(_grainSize / speed / 2);//指定速度での1/2グレイン辺りのサンプル数(grainSamplesを偶数にしたいのでこっちを先に計算)
            int grainSamples = grainHalfSamples * 2;
            int listcount = msec2SampleCount(msec);//ストリーミングに使う最終が44100なのでそれ用のバッファのカウント数(理想値)
            if (grainHalfSamples == 0 || msec <= 0)
            {
                mergesize = 0;
                return;
            }
            int grainCount = (int)(listcount / grainHalfSamples);//今回nグレイン分をリスト化する(半分ずつマージされるのでHalfSamples何個分かになる)

            outGrainCount = (double)listcount / grainSamples;//speedでmsec分再生すると何グレインになるか。25msec等速なら1グレイン
            if (reverse) outGrainCount = -outGrainCount;//逆再生ならマイナス値を返す

            grainCount++;
            mergesize = grainHalfSamples;
            int startindex;
            int endindex;
            if (!reverse)
            {
                startindex = _grainIndex;
                endindex = startindex + grainCount - 1;//_grainList.Count - 1;
                _grainIndex = (_grainIndex + grainCount) % _grainListL.Count;
                outGrainStart = startindex;
            }
            else
            {
                endindex = _grainIndex - 1;
                startindex = endindex - grainCount + 1;
                while (true)
                {
                    if (startindex < 0)
                    {
                        endindex += _grainListL.Count;
                        startindex += _grainListL.Count;
                    }
                    else
                    {
                        break;
                    }
                }
                _grainIndex = (_grainIndex - grainCount + _grainListL.Count * 100) % _grainListL.Count;
                outGrainStart = endindex;
            }
            outGrainNext = _grainIndex;

            int counter = startindex;
            //前半分 + 合体*n + 後ろ半分
            {
                for (int i = 0; i < grainHalfSamples; i++)
                {
                    outRawDataL.Add(_grainListL[counter][(int)(i * speed)]);//[i]の部分に速度を加味する必要
                    outRawDataR.Add(_grainListR[counter][(int)(i * speed)]);//[i]の部分に速度を加味する必要
                }
                while (true)
                {
                    counter++;
                    if (counter <= endindex)
                    {

                        int thisindex = (counter + _grainListL.Count * 2) % _grainListL.Count;
                        int preindex = (counter - 1 + _grainListL.Count * 2) % _grainListL.Count;

                        for (int i = 0; i < grainHalfSamples; i++)
                        {
                            outRawDataL.Add(StreamingBuffer.AddInt16(_grainListL[preindex][(int)(i * speed) + _grainHalfSize], _grainListL[thisindex][(int)(i * speed)]));//[i]の部分に速度を加味する必要
                            outRawDataR.Add(StreamingBuffer.AddInt16(_grainListR[preindex][(int)(i * speed) + _grainHalfSize], _grainListR[thisindex][(int)(i * speed)]));//[i]の部分に速度を加味する必要
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                for (int i = 0; i < grainHalfSamples; i++)
                {
                    int thisindex = (counter - 1 + _grainListL.Count * 2) % _grainListL.Count;
                    outRawDataL.Add(_grainListL[thisindex][(int)(i * speed) + _grainHalfSize]);//[i]の部分に速度を加味する必要
                    outRawDataR.Add(_grainListR[thisindex][(int)(i * speed) + _grainHalfSize]);//[i]の部分に速度を加味する必要
                }
            }
            if (reverse)
            {
                outRawDataL.Reverse();
                outRawDataR.Reverse();
            }
        }

        /// <summary>
        /// 逆位相(逆再生ではない)
        /// </summary>
        /// <param name="grainIndex"></param>
        /// <param name="Invert"></param>
        /// <param name="_grainList"></param>
        /// <returns></returns>
        private List<Int16> GetGrain(int grainIndex, bool Invert, List<Int16[]> _grainList)
        {
            List<Int16> rtnList = new List<short>();
            foreach (short s in _grainList[grainIndex])
            {
                if (Invert)
                {
                    rtnList.Add((short)-s);
                }
                else
                {
                    rtnList.Add(s);
                }
            }
            return rtnList;
        }

        public void MakeSampleFromGrainCountList(double speed, int grainCount, bool reverse, out int mergesize, out List<float> outRawDataL, out List<float> outRawDataR)
        {
            int ograinIndex = _grainIndex;
            outRawDataL = MakeSampleFromGrainCountList(speed, grainCount, reverse, out mergesize, _grainListL);
            _grainIndex = ograinIndex;
            outRawDataR = MakeSampleFromGrainCountList(speed, grainCount, reverse, out mergesize, _grainListR);
        }

        private List<float> MakeSampleFromGrainCountList(double speed, int grainCount, bool reverse, out int mergesize, List<Int16[]> _grainList)
        {
            List<float> rtnList = new List<float>();
            if (Math.Abs(speed) < 0.5 || grainCount == 0)
            {
                mergesize = 0;
                return rtnList;
            }

            int grainHalfSamples = (int)(_grainSize / speed / 2);//指定速度での1/2グレイン辺りのサンプル数(grainSamplesを偶数にしたいのでこっちを先に計算)
            int grainSamples = grainHalfSamples * 2;

            mergesize = grainHalfSamples;
            int startindex;
            int endindex;
            if (!reverse)
            {
                startindex = _grainIndex;
                endindex = startindex + grainCount - 1;//_grainList.Count - 1;
                _grainIndex = (_grainIndex + grainCount) % _grainList.Count;
            }
            else
            {
                endindex = _grainIndex - 1;
                startindex = endindex - grainCount + 1;
                if (startindex < 0)
                {
                    endindex += _grainList.Count * 100;
                    startindex += _grainList.Count * 100;
                }
                _grainIndex = (_grainIndex - grainCount + _grainList.Count * 100) % _grainList.Count;
            }


            int counter = startindex;

            for (int i = 0; i < grainHalfSamples; i++)
            {
                int thisindex = (counter + _grainList.Count * 2) % _grainList.Count;
                rtnList.Add(_grainList[thisindex][(int)(i * speed)]);//[i]の部分に速度を加味する必要
            }
            while (true)
            {
                counter++;
                if (counter <= endindex)
                {
                    int thisindex = (counter + _grainList.Count * 2) % _grainList.Count;
                    int preindex = (counter - 1 + _grainList.Count * 2) % _grainList.Count;

                    for (int i = 0; i < grainHalfSamples; i++)
                    {
                        rtnList.Add(StreamingBuffer.AddInt16(_grainList[preindex][(int)(i * speed) + _grainHalfSize], _grainList[thisindex][(int)(i * speed)]));//[i]の部分に速度を加味する必要
                    }
                }
                else
                {
                    break;
                }
            }
            for (int i = 0; i < grainHalfSamples; i++)
            {
                int thisindex = counter % _grainList.Count;
                rtnList.Add(_grainList[thisindex][(int)(i * speed) + _grainHalfSize]);//[i]の部分に速度を加味する必要
            }
            if (reverse)
            {
                rtnList.Reverse();
            }
            return rtnList;
        }


        
        #endregion

    }
}