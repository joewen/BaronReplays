using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaronReplays.JsonData
{
    public class FeaturedGameBanned
    {
        protected Int32 _championId;
        public Int32 championId
        {
            get
            {
                return _championId;
            }
            set
            {
                _championId = value;
            }
        }

        protected UInt32 _teamId;
        public UInt32 teamId
        {
            get
            {
                return _teamId;
            }
            set
            {
                _teamId = value;
            }
        }

        protected UInt32 _pickTurn;
        public UInt32 pickTurn
        {
            get
            {
                return _pickTurn;
            }
            set
            {
                _pickTurn = value;
            }
        }

    }
}
