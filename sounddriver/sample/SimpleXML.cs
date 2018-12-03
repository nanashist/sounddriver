using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;
using System.Text;


/// <summary>
/// XML保存するクラスのサンプル
/// 変数は全てpublicで宣言する。privateも可能だがファイルには残らない。
/// </summary>
public class SimpleXML
{

    public Nest nest;

    public string item1;
    /// <summary>
    /// Nestクラスは外に定義しても良い。ネストさせるとXML自体が階層構造になって見やすくなる。
    /// </summary>
    public class Nest
    {
        public string subitem1;
    }
}
