using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluorineFx;
using FluorineFx.AMF3;
using System.Windows;

namespace BaronReplays
{
    public class DetailStats
    {
        private Dictionary<String, UInt32> _statsCollection;
        public Dictionary<String, UInt32> StatsCollection
        {
            get
            {
                return _statsCollection;
            }
        }

        public UInt32 K
        {
            get
            {
                return _statsCollection["CHAMPIONS_KILLED"];
            }
        }
        public UInt32 D
        {
            get
            {
                return _statsCollection["NUM_DEATHS"];
            }
        }
        public UInt32 A
        {
            get
            {
                return _statsCollection["ASSISTS"];
            }
        }

        public UInt32 Level
        {
            get
            {
                return _statsCollection["LEVEL"];
            }
        }

        public UInt32 MinionsKilledTotal
        {
            get
            {
                return MinionsKilled + NeutralMinionsKilled;
            }
        }

        public UInt32 GoldEarned
        {
            get
            {
                return _statsCollection["GOLD_EARNED"];
            }
        }

        public String KGoldEarned
        {
            get
            {
                Double d = _statsCollection["GOLD_EARNED"];
                d /= 1000;
                return String.Format("${0:.0}K", d);
            }
        }

        public UInt32 WardPlaced
        {
            get
            {
                return _statsCollection["WARD_PLACED"];
            }
        }

        public UInt32 WardKilled
        {
            get
            {
                return _statsCollection["WARD_KILLED"];
            }
        }

        public UInt32 TurretsKilled
        {
            get
            {
                return _statsCollection["TURRETS_KILLED"];
            }
        }

        public UInt32 DamegeTaken
        {
            get
            {
                return _statsCollection["TOTAL_DAMAGE_TAKEN"];
            }
        }

        public UInt32 DamegeDealt
        {
            get
            {
                return _statsCollection["TOTAL_DAMAGE_DEALT_TO_CHAMPIONS"];
            }
        }

        public String KDA
        {
            get
            {
                return String.Format("{0,-2} / {1,-2} / {2,-2}", K, D, A);
            }
        }

        public String KDAValue
        {
            get
            {
                Double result = (double)(K + A) / (double)D;
                string output = "KDA ∞";
                if (!Double.IsPositiveInfinity(result))
                    return String.Format("KDA {0:0.0}", result);
                return output;
            }
        }

        public UInt32 MinionsKilled
        {
            get
            {
                return _statsCollection["MINIONS_KILLED"];
            }
        }

        public UInt32 NeutralMinionsKilled
        {
            get
            {
                return _statsCollection["NEUTRAL_MINIONS_KILLED"];
            }
        }

        public String MinionsKilledTotalString
        {
            get
            {
                return String.Format("{0} + {1}", MinionsKilled, NeutralMinionsKilled);
            }
        }

        public Boolean Win
        {
            get
            {
                return _statsCollection.ContainsKey("WIN");
            }
        }

        private UInt32[] _itemsArray;
        private void InitItemArray()
        {
            _itemsArray = new UInt32[7];
            String item = "ITEM";
            int nth = 0;
            for (int i = 0; i < 6; i++)
            {
                UInt32 number;
                if (_statsCollection.ContainsKey(item + i))
                    number = _statsCollection[item + i];
                else
                    number = 0;
                if (number != 0)
                    _itemsArray[nth++] = _statsCollection[item + i];
            }
            _itemsArray[6] = _statsCollection["ITEM6"];
        }

        public UInt32 Item0
        {
            get
            {
                if (_itemsArray == null)
                    InitItemArray();
                return _itemsArray[0];
            }
        }

        public UInt32 Item1
        {
            get
            {
                if (_itemsArray == null)
                    InitItemArray();
                return _itemsArray[1];
            }
        }
        public UInt32 Item2
        {
            get
            {
                if (_itemsArray == null)
                    InitItemArray();
                return _itemsArray[2];
            }
        }
        public UInt32 Item3
        {
            get
            {
                if (_itemsArray == null)
                    InitItemArray();
                return _itemsArray[3];
            }
        }
        public UInt32 Item4
        {
            get
            {
                if (_itemsArray == null)
                    InitItemArray();
                return _itemsArray[4];
            }
        }
        public UInt32 Item5
        {
            get
            {
                if (_itemsArray == null)
                    InitItemArray();
                return _itemsArray[5];
            }
        }
        public UInt32 Item6
        {
            get
            {
                if (_itemsArray == null)
                    InitItemArray();
                return _itemsArray[6];
            }
        }


        public DetailStats(ArrayCollection dStats)
        {
            _statsCollection = new Dictionary<String, UInt32>();
            foreach (ASObject obj in dStats)
            {
                _statsCollection.Add(obj["statTypeName"] as String, UInt32.Parse(obj["value"].ToString()));
            }
        }
    }
}
