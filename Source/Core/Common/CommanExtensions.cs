using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Common
{
    public static class DecimalRoundOff
    {

        public static decimal amountToFixed(decimal ? Amount, int ? DecimalPlaces)
        {
            if (DecimalPlaces.HasValue)
                return (decimal.Round(Amount ?? 0, DecimalPlaces.Value,MidpointRounding.AwayFromZero));
            else
                return (decimal.Round(Amount ?? 0, 2, MidpointRounding.AwayFromZero));
        }


    }
}
