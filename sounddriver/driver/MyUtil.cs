using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static    class MyUtil
{
    /// <summary>
    /// msecをticksに変換する (1 ミリ秒 = 10000 タイマ刻み) 
    /// </summary>
    /// <param name="msec"></param>
    /// <returns></returns>
    public static long ticks2msec = 10000;

    /// <summary>
    /// ticksを秒にするにはこれで割る
    /// 秒をticksにするにはこれをかける
    /// </summary>
    public static long ticks2sec = 10000000;
}
