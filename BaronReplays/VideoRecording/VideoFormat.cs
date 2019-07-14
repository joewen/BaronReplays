using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaronReplays.VideoRecording
{
    public class VideoFormat    
    {
        public Int32 Width
        {
            get;
            set;
        }

        public Int32 Height
        {
            get;
            set;
        }

        public Int32 Bits
        {
            get;
            set;
        }

        public ResolutionRatio GetResolutionRatio
        {
            get
            {
                int heightRatio = Height / (Width / 16);
                ResolutionRatio result = ResolutionRatio.UNKONW;
                switch (heightRatio)
                { 
                    case 9:
                        result = ResolutionRatio.R16X9;
                        break;
                    case 10:
                        result = ResolutionRatio.R16X10;
                        break;
                    case 12:
                        result = ResolutionRatio.R4X3;
                        break;
                }
                return result;
            }
        }

        public enum ResolutionRatio
        {
            UNKONW = 0,
            R16X9 = 9,
            R16X10 = 10,
            R4X3 = 12,
        }

    }
}
