using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BaronReplays
{
    public class ProcessInfo
    {
        public Process Process
        {
            get;
            set;
        }

        public String ExecutablePath
        {
            get;
            set;
        }


        public String CommandLine
        {
            get;
            set;
        }
    }
}
