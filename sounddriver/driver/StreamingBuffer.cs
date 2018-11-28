using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio;
using NAudio.Wave;
//初期化して
//streamingbuffer = new StreamingBuffer(waveformat);
//バッファに詰め込んでいく。詰め込んだらすぐ鳴る
//streamingbuffer.AddInt16Buffer(first.RawDataL, first.RawDataR);
//こいつを継続的に呼びまくる。
//streamingbuffer.Streaming();


/// <summary>
/// ステレオ音声ストリーミングバッファー(16bitPCM)のIWaveProvider
/// </summary>
public class StreamingBuffer
{
    private const double _streambuffermsec = 500;//NAUDIO側のバッファがこの値以下ならこの値分バッファに詰め込む
    //コメント修正 旧小さいほど良いが正常動作する限界値をセットしたい。この値を下回るタイミングでbufwaveproviderにバッファを追加していく

    private int _SamplingRate;
    private int _Channels;
    /// <summary>
    /// AddSampleがバイト配列しか受け付けないのでIEEEではなくPCMで無いと扱えない
    /// </summary>
    private BufferedWaveProvider bufwaveprovider16;
    private VolumeWaveProvider16 volume;
    private Int16Buffer[] ChannelBuffers;

    #region"プロパティ

    /// <summary>
    /// ストリーミングバッファの音量(R/W)
    /// </summary>
    public float Volume { get { return volume.Volume; } set { volume.Volume = value; } }
    /// <summary>
    /// サンプリングレート
    /// </summary>
    public int SampleRate { get { return volume.WaveFormat.SampleRate; } }
    /// <summary>
    /// チャンネル数
    /// </summary>
    public int Channels { get { return volume.WaveFormat.Channels; } }
    /// <summary>
    /// ミキサーに渡す(R)
    /// </summary>
    public IWaveProvider OutputWaveProvider { get { return new Wave16ToFloatProvider(volume); } }

    public TimeSpan BufferedDuration { get { return bufwaveprovider16.BufferedDuration; } }
    #endregion

    public StreamingBuffer(WaveFormat waveformat)
    {
        _SamplingRate = waveformat.SampleRate;
        _Channels = waveformat.Channels;
        bufwaveprovider16 = new BufferedWaveProvider(new WaveFormat(_SamplingRate, 16, _Channels));

        volume = new VolumeWaveProvider16(bufwaveprovider16);
        volume.Volume = 0.1F;
        ChannelBuffers = new Int16Buffer[2];
        ChannelBuffers[0] = new Int16Buffer();
        ChannelBuffers[1] = new Int16Buffer();
        
    }

    /// <summary>
    /// ストリーミングバッファにデータを追加
    /// </summary>
    /// <param name="lbuf"></param>
    /// <param name="rbuf"></param>
    /// <param name="mergesize"></param>
    public void AddFloatBuffer(List<float> lbuf, List<float> rbuf)
    {
        AddInt16Buffer(floatList2int16List(lbuf), floatList2int16List(rbuf));
    }

    /// <summary>
    /// ストリーミングバッファにデータを追加
    /// </summary>
    /// <param name="lbuf"></param>
    /// <param name="rbuf"></param>
    /// <param name="mergesize"></param>
    public void AddInt16Buffer(List<Int16> lbuf, List<Int16> rbuf)
    {
        ChannelBuffers[0].AddBuffer(lbuf);
        ChannelBuffers[1].AddBuffer(rbuf);
    }



    public int GetBufferCount()
    {
        return ChannelBuffers[0].Buffercount;
    }

    /// <summary>
    /// 頻繁に呼んでバッファを補充する
    /// </summary>
    /// <returns></returns>
    public void Streaming()
    {
        if (ChannelBuffers[0].Buffercount > 0)
        {
            double needsmsec = _streambuffermsec - bufwaveprovider16.BufferedDuration.TotalMilliseconds;
            string s;
            s = "Streaming  ";
            s += "WaveProvider16=" + bufwaveprovider16.BufferedDuration.TotalMilliseconds.ToString();

            if (needsmsec > 0)
            {
                Buffer2NAudio(_streambuffermsec);
                s += "    500msec送信";
            }
            Test.Print(s);
        }
    }

    /// <summary>
    /// 浮動小数点-1～1のリストをInt16(-32767～32767)に変換
    /// </summary>
    /// <param name="fList"></param>
    /// <returns></returns>
    private List<Int16> floatList2int16List(List<float> fList)
    {
        List<Int16> rtn = new List<Int16>();
        foreach (float f in fList)
        {
            if (f > 1)
                rtn.Add((Int16)(32767));
            else if (f < -1)
                rtn.Add((Int16)(-32767));
            else
                rtn.Add((Int16)(f * 32767));
        }
        return rtn;
    }
    /// <summary>
    /// Streaming()のバッファ管理を使用せず直接NAudioにデータを流す
    /// </summary>
    /// <param name="lbuf"></param>
    /// <param name="rbuf"></param>
    public void Direct2NAudioBuffer(List<float> lbuf, List<float> rbuf)
    {
        Direct2NAudioBuffer(floatList2int16List(lbuf), floatList2int16List(rbuf));
    }

    /// <summary>
    /// Streaming()のバッファ管理を使用せず直接NAudioにデータを流す
    /// </summary>
    /// <param name="lbuf"></param>
    /// <param name="rbuf"></param>
    public void Direct2NAudioBuffer(List<Int16> lbuf, List<Int16> rbuf)
    {

        List<Int16> mixbuf = new List<Int16>();
        for (int i = 0; i < lbuf.Count; i++)
        {
            mixbuf.Add(lbuf[i]);
            if (_Channels == 2)
            {
                mixbuf.Add(rbuf[i]);
            }
        }

        Int16[] intarray;
        intarray = mixbuf.ToArray();


        byte[] b;
        b = new byte[intarray.Length * 2 - 1 + 1];
        UInt16 tempUint;
        int cnt = 0;

        for (var i = 0; i <= intarray.Length - 1; i++)
        {
            if (intarray[i] >= 0)
                tempUint = (UInt16)intarray[i];
            else
                tempUint = (UInt16)(65536 + intarray[i]);//-1=ffff=65535 -2=fffe=65534...
            b[cnt] = (byte)(tempUint % 256);
            cnt += 1;
            b[cnt] = (byte)(tempUint / 256);
            cnt += 1;
        }

        bufwaveprovider16.AddSamples(b, 0, b.Length);

    }

    /// <summary>
    /// NAUDIOのバッファにデータを流す
    /// </summary>
    /// <param name="intarray"></param>
    /// <remarks></remarks>
    private void Buffer2NAudio(double needsmsec)
    {
        int samplecount = (int)(_SamplingRate * needsmsec / 1000);
        List<Int16> lbuf = ChannelBuffers[0].GetBuffer(samplecount);
        List<Int16> rbuf = new List<short>();
        if (_Channels == 2)
        {
            rbuf = ChannelBuffers[1].GetBuffer(samplecount);
        }
        Direct2NAudioBuffer(lbuf, rbuf);

    }

    #region "ストリーミングバッファ管理用ローカルクラス
    /// <summary>
    /// 1チャンネル分のストリーミングRAWデータを管理するクラス
    /// ストリーミングに使うbufwaveproviderはFloat型が無理でバイト配列(16bit)のみ受け取れる
    /// </summary>
    private class Int16Buffer
    {
        private List<Int16> buffList;

        public Int16Buffer()
        {
            buffList = new List<Int16>();
        }
        #region"公開関数"

        /// <summary>
        /// このチャンネルにWAVE_RAWデータのリストを追加する
        /// </summary>
        /// <param name="lst"></param>
        /// <remarks></remarks>
        public void AddBuffer(List<Int16> lst)
        {
            buffList.AddRange(lst);
            return;
        }

        /// <summary>
        /// 残りバッファを捨てて初期化
        /// </summary>
        /// <remarks></remarks>
        public void ClearBuffer()
        {
            buffList.Clear();
        }

        /// <summary>
        /// 未再生のバッファ数
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public int Buffercount
        {
            get
            {
                return buffList.Count;
            }
        }

        /// <summary>
        /// バッファを頭から指定サイズ取り出す（残り少なければ全部）
        /// </summary>
        /// <param name="GetBytes"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public List<Int16> GetBuffer(int GetBytes)
        {
            List<Int16> templist;
            if (buffList.Count > GetBytes)
            {
                templist = buffList.GetRange(0, GetBytes);
                buffList.RemoveRange(0, GetBytes);
            }
            else
            {
                templist = buffList.GetRange(0, buffList.Count);
                buffList.RemoveRange(0, buffList.Count);
            }
            return templist;
        }
        #endregion



    }
    #endregion

    #region"ローカル関数

    /// <summary>
    /// 足してInt16に収める。超えた値はMAXにしてしまう
    /// </summary>
    /// <param name="src"></param>
    /// <param name="dst"></param>
    /// <returns></returns>
    public static Int16 AddInt16(Int16 src, Int16 dst)
    {
        Int32 sum = src + dst;
        if (sum > 32767)
            return 32767;
        else if (sum < -32767)
            return -32767;
        else
            return (Int16)sum;
    }

    #endregion

}
