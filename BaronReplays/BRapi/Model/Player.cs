using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaronReplays.BRapi.Model
{
    class Player
    {
        [JsonProperty(PropertyName = "summonerId")]
        public int SummonerId;

        [JsonProperty(PropertyName = "summonerInternalName")]
        public string SummonerInternalName;

        [JsonProperty(PropertyName = "championId")]
        public int ChampionId;

        [JsonProperty(PropertyName = "spell1Id")]
        public int Spell1Id;

        [JsonProperty(PropertyName = "spell2Id")]
        public int Spell2Id;

        [JsonProperty(PropertyName = "selectedSkinIndex")]
        public int SelectedSkinIndex;

    }
}
