using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaronReplays.RiotAPI
{
    class Constants
    {
        public readonly static Dictionary<int, string> QueueType = new Dictionary<int, string>()
        {
            {0,"CUSTOM"},
            {2,"NORMAL_5x5_BLIND"},
            {7,"BOT_5x5"},
            {31,"BOT_5x5_INTRO"},
            {32,"BOT_5x5_BEGINNER"},
            {33,"BOT_5x5_INTERMEDIATE"},
            {8,"NORMAL_3x3"},
            {14,"NORMAL_5x5_DRAFT"},
            {16,"ODIN_5x5_BLIND"},
            {17,"ODIN_5x5_DRAFT"},
            {25,"BOT_ODIN_5x5"},
            {4,"RANKED_SOLO_5x5"},
            {9,"RANKED_PREMADE_3x3"},
            {6,"RANKED_PREMADE_5x5"},
            {41,"RANKED_TEAM_3x3"},
            {42,"RANKED_TEAM_5x5"},
            {52,"BOT_TT_3x3"},
            {61,"GROUP_FINDER_5x5"},
            {65,"ARAM_5x5"},
            {70,"ONEFORALL_5x5"},
            {72,"FIRSTBLOOD_1x1"},
            {73,"FIRSTBLOOD_2x2"},
            {75,"SR_6x6"},
            {76,"URF_5x5"},
            {83,"BOT_URF_5x5"},
            {91,"NIGHTMARE_BOT_5x5_RANK1"},
            {92,"NIGHTMARE_BOT_5x5_RANK2"},
            {93,"NIGHTMARE_BOT_5x5_RANK5"},
            {96,"ASCENSION_5x5"},
            {98,"HEXAKILL"},
            {300,"KING_PORO_5x5"},
            {310,"COUNTER_PICK"},
            {313,"BILGEWATER_5x5"},
            {400,"TEAM_BUILDER_DRAFT_UNRANKED_5x5"},
            {410,"TEAM_BUILDER_DRAFT_RANKED_5x5"},
            {420,"TEAM_BUILDER_RANKED_SOLO"},
            {440,"RANKED_FLEX_SR"},
        };
    }
}
