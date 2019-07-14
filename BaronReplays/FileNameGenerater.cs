using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace BaronReplays
{
    public static class FileNameGenerater
    {
        public static String GenerateFilename(LoLRecorder recoder)
        {
            if (!recoder.selfGame)
            {
                return Utilities.GetString("Spectate") as String + "-" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.lpr");
            }

            String format = Properties.Settings.Default.FileNameFormat;
            format = ReplaceGeneralProperty(recoder, format);
            if (recoder.record.HasResult)
            {
                format = ReplaceEndOfGameStatsProperty(recoder, format);
            }
            format = format.Replace('<', '(');
            format = format.Replace('>', ')');
            return format;
        }

        public static String ReplaceParticipantProperty(LoLRecorder recoder, String format)
        {
            try
            {
                int nth = 0;
                if (recoder.record.gameStats.Players.Count == 0)
                    return format;
                for (; nth < recoder.record.gameStats.Players.Count; nth++)
                {
                    if (recoder.record.gameStats.Players[nth].SummonerName == recoder.selfPlayerInfo.playerName)
                        break;
                }

                PlayerStats selfStats = recoder.record.gameStats.Players[nth];
                format = format.Replace("<Kill>", selfStats.Statistics.K.ToString());
                format = format.Replace("<Death>", selfStats.Statistics.D.ToString());
                format = format.Replace("<Assist>", selfStats.Statistics.A.ToString());
                format = format.Replace("<KDA>", string.Format("{0:N1}", selfStats.Statistics.KDAValue));
                format = format.Replace("<WinLose>", selfStats.Statistics.Win ? Utilities.GetString("Win") as string : Utilities.GetString("Lose") as string);
            }
            catch (Exception)
            { 

            }
            return format;
        }

        public static String ReplaceEndOfGameStatsProperty(LoLRecorder recoder, String format)
        {
            format = format.Replace("<Map>", Utilities.GetString(recoder.record.gameStats.GameMode) as string);
            if (recoder.record.gameStats.Players != null)
            {
                format = ReplaceParticipantProperty(recoder, format);
            }
            return format;
        }

        public static String ReplaceGeneralProperty(LoLRecorder recoder, String format)
        {
            if (recoder.selfPlayerInfo != null)
            {
                format = format.Replace("<SummonerName>", recoder.selfPlayerInfo.playerName);
                format = format.Replace("<Champion>", LoLStaticData.Champion.Instance.GetNameByKey(recoder.selfPlayerInfo.championName));
            }
            DateTime dt = DateTime.Now;
            format = format.Replace("<Year>", dt.Year.ToString());
            format = format.Replace("<Month>", dt.Month.ToString());
            format = format.Replace("<Day>", dt.Day.ToString());
            format = format.Replace("<Hour>", dt.Hour.ToString("00"));
            format = format.Replace("<Minute>", dt.Minute.ToString("00"));
            format = format.Replace("<Second>", dt.Second.ToString("00"));
            return format;
        }


    }
}
