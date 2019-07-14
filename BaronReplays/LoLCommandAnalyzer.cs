using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BaronReplays
{
    public class LoLCommandAnalyzer
    {
        private GameInfo _gameinfo;

        public Boolean IsSuccess;
        public LoLCommandAnalyzer(String str)
        {
            
            Regex gameIdRegex = new Regex("spectator (?<ADDR>[A-Za-z0-9\\.]+:*[0-9]*) (?<KEY>.{32}) (?<GID>[0-9]+) (?<PF>[A-Z0-9_]+)", RegexOptions.IgnoreCase);
            Match match = gameIdRegex.Match(str);
            IsSuccess = match.Success;
            if (match.Success)
            {
                _gameinfo = new GameInfo();
                _gameinfo.ServerAddress = match.Groups["ADDR"].Value;
                _gameinfo.ObKey = match.Groups["KEY"].Value;
                _gameinfo.GameId = long.Parse(match.Groups["GID"].Value);
                _gameinfo.PlatformId = match.Groups["PF"].Value;

                if (str.Substring(0, 15).StartsWith("lrf://spectator"))
                {
                    if (!_gameinfo.ServerAddress.Contains(':'))
                    {
                        _gameinfo.ServerAddress = _gameinfo.ServerAddress + ":8080";
                    }
                }
            }
        }

        public GameInfo GetGameInfo()
        {
            return _gameinfo;
        }

    }
}
