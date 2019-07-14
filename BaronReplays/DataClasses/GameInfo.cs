using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaronReplays
{
    public class GameInfo
    {
        public GameInfo(string obUrl, string platform, long gameId, String obKey)
        {
            ServerAddress = obUrl;
            ObKey = obKey;
            GameId = gameId;
            PlatformId = platform;
        }

        public GameInfo()
        {

        }


        public String ServerAddress
        {
            get;
            set;
        }
        public String ObKey
        {
            get;
            set;
        }
        public long GameId
        {
            get;
            set;
        }
        public String PlatformId
        {
            get;
            set;
        }

        public String BRUrl
        {
            get
            {
                return String.Format("BaronReplays://spectator {0} {1} {2} {3}", ServerAddress, ObKey, GameId, PlatformId);
            }
        }

        public String CommandLine
        {
            get
            {
                return String.Format("\"{0}\" \"\" \"\" \"\" \"spectator {1} {2} {3} {4}\"",Properties.Settings.Default.LoLGameExe, ServerAddress, ObKey, GameId, PlatformId);
            }
        }

    }
}
