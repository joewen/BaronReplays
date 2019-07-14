using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BaronReplays
{
    class GameInfoFinder
    {
        private static uint _lastTimeAddress = 0;
        private LoLExeData exeData;
        private string obKey;
        public string ObKey
        {
            get
            {
                return obKey;
            }
        }


        private long gameId;
        public long GameId
        {
            get
            {
                return gameId;
            }
        }


        public GameInfoFinder(LoLExeData data)
        {
            exeData = data;
        }


        public bool DoWork()
        { 
            GetObKeyFormLoLClientMemory();
            return !(obKey == null);
        }

        public string GetSummonerName()
        {
            ProcessMemory pm = new ProcessMemory();
            if (!pm.openProcess("LolClient"))
                Logger.Instance.WriteLog("Open process failed");
            pm.recordMemorysInfo(false);
            string prefix = "@sec.pvp.net/";
            uint[] address = pm.findString(prefix, Encoding.ASCII, 1);
                
            byte[] b = pm.readMemory((uint)(address[0] + prefix.Length), 64);

            pm.closeProcess();

            int endPos = 0;
            while (b[endPos] != '\'' && b[endPos] != 0)
            {
                endPos++;
            }
            string name = Encoding.UTF8.GetString(b, 0, endPos);
            Logger.Instance.WriteLog("Summoner name is " + name);
            return name;
        }

        
        private void GetObKeyFormLoLClientMemory()
        {
            Regex gameIdRegex = new Regex(@" [0-9]+ (?<KEY>.+) [0-9]+", RegexOptions.IgnoreCase);
            Match match = gameIdRegex.Match(exeData.CommandLine);
            ProcessMemory pm = new ProcessMemory();
            if (!pm.openProcess("LolClient"))
                Logger.Instance.WriteLog("Open process failed");
            if (_lastTimeAddress != 0)
            {
                FindOnKeyFromLastTimeAddress(pm);
            }
            if (obKey == null)
            {
                pm.recordMemorysInfo(false);
                uint[] result = pm.findString(match.Groups["KEY"].Value, Encoding.ASCII, 1);
                if (!FindObkeyInMemory(pm, result))
                {
                    result = pm.findString(match.Groups["KEY"].Value, Encoding.ASCII);
                    FindObkeyInMemory(pm, result);
                }
            }
            pm.closeProcess();
        }

        private Boolean FindOnKeyFromLastTimeAddress(ProcessMemory proc)
        {
            return FindObkeyInMemory(proc, new uint[] { _lastTimeAddress });
        }

        private Boolean FindObkeyInMemory(ProcessMemory proc, uint[] addresses)
        {
            for (int i = 0; i < addresses.Length; i++)
            {
                byte[] memContent = proc.readMemory(addresses[i], 256);
                if (AnalyzeObKeyInMemoryBytesMethod1(memContent))
                {
                    _lastTimeAddress = addresses[i];
                    byte[] gid = proc.readMemory(addresses[i] + 25, 8).Reverse().ToArray();
                    gameId = (long)BitConverter.ToDouble(gid, 0);
                    return true;
                }
            }
            return false;
        }


        private bool AnalyzeObKeyInMemoryBytesMethod1(byte[] memBytes)
        {
            try
            {
                Int16 currloc = 0;
                StringBuilder sb = new StringBuilder();
                List<Int16> endPositions = new List<Int16>();
                bool splitted = false;
                while (currloc < memBytes.Length)
                {
                    if (memBytes[currloc] > 32 && memBytes[currloc] < 125)
                    {
                        sb.Append((char)memBytes[currloc]);
                        splitted = false;

                    }
                    else
                    {
                        if (!splitted)
                        {
                            endPositions.Add(currloc);
                            sb.Append((char)32);
                            splitted = true;
                        }
                    }
                    currloc++;
                }
                string[] segments = sb.ToString().Split(' ');



                for (int i = 0; i < segments.Length; i++)
                {
                    if (segments[i].Length == 33)
                    {
                        if (segments[i][0] == 'A')
                        {
                            obKey = segments[i].Substring(1, 32);
                            //return true;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
            return !(obKey == null);
            //return false;
        }
    }
}
