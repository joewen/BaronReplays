using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaronReplays
{
    public interface IFeaturedGamesQuery
    {
        void RefreshDone(Boolean isSuccess);
    }
}
