using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XMLFile
{
    public class ScratchXML
    {
        public string ScratchFile0;
        public int Index0;

        public string ScratchFile1;
        public int Index1;

        public string ScratchFile2;
        public int Index2;

        public string ScratchFile3;
        public int Index3;

        public bool Active(int index)
        {
            if (index == 1)
                return !String.IsNullOrEmpty( ScratchFile1);
            else if (index == 2)
                return !String.IsNullOrEmpty(ScratchFile2);
            else if (index == 3)
                return !String.IsNullOrEmpty(ScratchFile3);
            else
                return !String.IsNullOrEmpty(ScratchFile0);
        }
        public string ScratchFile(int index)
        {
            if (index == 1)
                return ScratchFile1;
            else if (index == 2)
                return ScratchFile2;
            else if (index == 3)
                return ScratchFile3;
            else
                return ScratchFile0;
        }

        public int GrainIndex(int index)
        {
            if (index == 1)
                return Index3;
            else if (index == 2)
                return Index2;
            else if (index == 3)
                return Index1;
            else
                return Index0;

        }

    }
}
