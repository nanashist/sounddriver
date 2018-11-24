using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio;
using NAudio.Wave;
using System.Diagnostics;
/// <summary>
/// IeeeFloat,44100Hz,2ch固定のASIO対応音声ミキサー
/// </summary>
public static class SoundDriver
{
    private static MixingWaveProvider32 mixer;
    private static IWavePlayer waveOut;
    private static WaveFormat waveformat;
    private static Dictionary<string, IWaveProvider> DctWaveProvider;

    /// <summary>
    /// IeeeFloat,44100Hz,2ch(固定)でミキサーの初期化
    /// </summary>
    public static void Init()
    {
        mixer = new MixingWaveProvider32();

        //waveOut = new AsioOut();//NAudio.CoreAudioApi.AudioClientShareMode.Exclusive,100);
        Test.Print("ASIOに変更");
        waveOut = new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, 100);
        waveOut.Init(mixer);
        waveformat = WaveFormatExtensible.CreateIeeeFloatWaveFormat(44100, 2);
        

        DctWaveProvider = new Dictionary<string, IWaveProvider>();
    }
    /// <summary>
    /// 44100Hz,2chのファイル読み込み("main"で追加される)
    /// </summary>
    /// <param name="filename"></param>
    public static void MainFileLoad(string filename)
    {
        IWaveProvider FloatStereo44100Provider;
        AudioFileReader reader;
        reader = new AudioFileReader(filename);
        Test.Print("Volume消す");
        reader.Volume = 0.01F;

        IWaveProvider stereo;
        if (reader.WaveFormat.Channels == 1)
        {
            if (reader.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
            {
                //NAudio.Wave.SampleProviders.MonoToStereoSampleProvider s = new NAudio.Wave.SampleProviders.MonoToStereoSampleProvider(reader);
                stereo = new Wave16ToFloatProvider(new MonoToStereoProvider16(new WaveFloatTo16Provider(reader)));
                WaveFormatConversionProvider conv = new WaveFormatConversionProvider(new WaveFormat(44100, 2), stereo);
            }
            else if (reader.WaveFormat.Encoding == WaveFormatEncoding.Pcm)
            {
                stereo = new Wave16ToFloatProvider(new MonoToStereoProvider16(reader));
            }
            else
            {
                return;
            }

        }
        else
        {
            stereo = reader;
        }

        FloatStereo44100Provider = stereo;//最終的にこの形式に統一44100にするかどうかは検討の余地あり
        
        SoundDriver.AddWaveProvider(FloatStereo44100Provider, "main");
    }

    /// <summary>
    /// 再生開始
    /// </summary>
    public static void Play()
    {
        waveOut.Play();
    }

    public static void Stop()
    {
        waveOut.Stop();
    }
    /// <summary>
    /// 再生状態取得
    /// </summary>
    /// <returns></returns>
    public static PlaybackState PlaybackState()
    {
        return waveOut.PlaybackState;
    }

    /// <summary>
    /// ミキサーに音声の追加
    /// </summary>
    /// <param name="waveprovider"></param>
    /// <param name="name"></param>
    public static void AddWaveProvider(IWaveProvider waveprovider, string name)
    {
        
        //Wave16ToFloatProviderとか使ってIeeeFloatにしないとだめ
        if (waveprovider.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
            return;
        if (waveprovider.WaveFormat.SampleRate != waveformat.SampleRate
            || waveprovider.WaveFormat.Channels != waveformat.Channels)
            return;
        if (DctWaveProvider.ContainsKey(name))
        {
            RemoveWaveProvider(name);
        }
        DctWaveProvider.Add(name, waveprovider);
        mixer.AddInputStream(waveprovider);
    }

    /// <summary>
    /// ミキサーから音声の削除(waveprovider指定)
    /// </summary>
    /// <param name="waveprovider"></param>
    public static void RemoveWaveProvider(IWaveProvider waveprovider)
    {
        mixer.RemoveInputStream(waveprovider);
    }
    /// <summary>
    /// ミキサーから音声の削除(名称指定)
    /// </summary>
    /// <param name="waveprovider"></param>
    public static void RemoveWaveProvider(string name)
    {
        if (DctWaveProvider.ContainsKey(name))
        {
            mixer.RemoveInputStream(DctWaveProvider[name]);
            DctWaveProvider.Remove(name);
        }
    }

    public static void Dispose()
    {
        waveOut.Stop();
    }

    public static IWaveProvider GetWaveProvider(string name)
    {
        if (DctWaveProvider.ContainsKey(name))
        {
            return DctWaveProvider[name];
        }
        else
        {
            return null;
        }
    }
}
