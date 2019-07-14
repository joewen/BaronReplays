using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace BaronReplays
{
    public class LoLExeData : ProcessInfo
    {
        public Process LoLProcess
        {
            get;
            set;
        }

        public String LogFilePath
        {
            get;
            set;
        }

        public String LogContent
        {
            get
            {
                try
                {
                    return Utilities.ReadFileTextSafety(LogFilePath);
                }
                catch (Exception)
                { 
                    
                }
                return null;
            }
        }

        public GameInfo MatchInfo
        {
            get;
            set;
        }

        public bool IsSpectatorMode
        {
            get
            {
                return CommandLine.Contains("spectator");
            }
        }

        public int SummonerId
        {
            get
            {
                if (IsSpectatorMode)
                    throw new Exception("觀察者模式沒有招喚師Id");
                else
                {
                    string[] tokens = CommandLine.Split(new char[] { '\"', ' ' });
                    foreach(string s in tokens.Reverse())
                    {
                        int id = 0;
                        if (int.TryParse(s, out id))
                        {
                            return id;
                        }
                    }
                    return 0;
                }
            }
        }


    }
}
