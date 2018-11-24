using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KeyBoard
{

    
    public class KeyInput
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetAsyncKeyState(int vKey);

        public static bool GetKeyState(int vKey)
        {
            int i = GetAsyncKeyState(vKey);
            if (i == -32767) return true;
            if (i == -32768) return true;
            return false;
            //return (i != 0);//押されていればTrue
            //return ((i & 1) == 1);
        }

        public static List<Keys> GetPressedKeyList()
        {
            List<Keys> rtnlist = new List<Keys>();
            for (int i = 1; i <= 256; i++)
            {
                if (KeyBoard.KeyInput.GetKeyState(i))
                {
                    rtnlist.Add((Keys)i);
                }
            }
            return rtnlist;
        }
    }


}
