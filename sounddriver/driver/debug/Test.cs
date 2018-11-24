using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
public class Test
{
    private static string logfilename = "sounddriver.log";

    static StreamWriter tw;
    private static bool bInit = false;
    private static void Init()
    {
        if (!bInit)
        {
            tw =new StreamWriter(logfilename, true, System.Text.Encoding.Default);

            //自分自身のバージョン情報を取得する
            System.Diagnostics.FileVersionInfo ver =
                System.Diagnostics.FileVersionInfo.GetVersionInfo(
                System.Reflection.Assembly.GetExecutingAssembly().Location);
            //結果を表示
            tw.WriteLine("起動..." + ver);

            bInit=true;
        }
        
    }

    public static void Print(string s)
    {
        //Debug.Print((new DateTimeEx(DateTime.Now).Millis).ToString("D6") + "  " + s);
        Init();
        tw.WriteLine((new DateTimeEx(DateTime.Now).Millis).ToString("D6") + "  " + s);
    }

    public static void Close()
    {
        if (bInit)
            tw.Close();
    }
}
