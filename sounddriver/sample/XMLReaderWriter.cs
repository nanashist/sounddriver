using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;
using System.Text;

/// <summary>
/// リストを含まないクラスのpublic変数を一括でXML保存、読み込みするクラス
/// object rtn=new 目的のクラス;
/// XML.Read(file,ref rtn);
/// 目的のクラス class=(目的のクラス)rtn;
/// みたいにして使う
/// </summary>
public static class XML
{
    /// <summary>
    /// XML保存
    /// </summary>
    /// <param name="xmlFilename"></param>
    public static void Write(string xmlFilename, object Obj)
    {
        //XMLファイルに保存
        System.Xml.Serialization.XmlSerializer serializer =
            new System.Xml.Serialization.XmlSerializer(Obj.GetType());
        StreamWriter sw =
            new StreamWriter(xmlFilename, false, new UTF8Encoding(false));
        serializer.Serialize(sw, Obj);
        sw.Close();
    }

    /// <summary>
    /// XML読み込み
    /// </summary>
    /// <param name="xmlFilename"></param>
    /// <returns></returns>
    public static void Read(string xmlFilename, ref object rtnObj)
    {
        //object rtn = new SimpleXML();
        if (System.IO.File.Exists(xmlFilename))
        {
            try
            {
                //XMLファイルから復元
                System.Xml.Serialization.XmlSerializer serializer =
                    new System.Xml.Serialization.XmlSerializer(rtnObj.GetType());
                StreamReader sr = new StreamReader(xmlFilename, new UTF8Encoding(false));
                rtnObj = serializer.Deserialize(sr);
                sr.Close();
                return;
            }
            catch (System.OverflowException err)
            {
                Console.WriteLine(err.ToString());
            }
        }
        else
        {
        }
        rtnObj = null;
    }

}