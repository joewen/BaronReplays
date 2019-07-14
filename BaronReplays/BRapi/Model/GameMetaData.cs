using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaronReplays.BRapi.Model
{
    class GameMetaData
    {
        [JsonProperty(PropertyName = "game_id")]
        public Int64 GameId;

        [JsonProperty(PropertyName = "platform_id")]
        public string PlatformId;

        [JsonProperty(PropertyName = "observer_server_ip")]
        public string ObserverServerIp;

        [JsonProperty(PropertyName = "observer_server_port")]
        public int ObserverServerPort;

        [JsonProperty(PropertyName = "observer_encryption_key")]
        public string ObserverEncryptionKey;

        [JsonProperty(PropertyName = "game_type_config_id")]
        public int GameTypeConfigId;

        [JsonProperty(PropertyName = "teamOne")]
        public Player[] TeamOne;

        [JsonProperty(PropertyName = "teamTwo")]
        public Player[] TeamTwo;
    }
}
