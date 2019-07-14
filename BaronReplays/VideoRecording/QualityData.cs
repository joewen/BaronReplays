using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaronReplays
{
    public class QualityData
    {
        public QualityData(String name, String size, Boolean isRecommended,int rate)
        {
            Name = name;
            Size = size;
            IsRecommended = isRecommended;
            BitRate = rate;
        }

        private String name;
        public String Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        private String size;
        public String Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value;
            }
        }

        private Boolean isRecommended;
        public Boolean IsRecommended
        {
            get
            {
                return isRecommended;
            }
            set
            {
                isRecommended = value;
            }
        }

        private int bitRate;
        public int BitRate
        {
            get
            {
                return bitRate;
            }
            set
            {
                bitRate = value;
            }
        }
    }
}
