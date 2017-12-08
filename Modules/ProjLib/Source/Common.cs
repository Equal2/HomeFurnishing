using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjLib
{
    public enum ChargeTypesEnum
    {

        Amount = 5,
        SubTotal = 4,
        RoundOFF = 3,
        SalesATax = 2,
        SalesTax = 1
    }
    public enum RateTypeEnum
    {
        Rate = 1,
        Percentage = 2,
        Na = 3,
    }

    public class ChargeCodeConstants
    {
        public const string Incentive = "INCENT";
        public const string Penalty = "PENALTY";
    }

    public static class DecimalRoundOff
    {
        public static decimal amountToFixed(decimal? Amount, int? DecimalPlaces)
        {
            if (DecimalPlaces.HasValue)
                return (decimal.Round(Amount ?? 0, DecimalPlaces.Value, MidpointRounding.AwayFromZero));
            else
                return (decimal.Round(Amount ?? 0, 2, MidpointRounding.AwayFromZero));
        }
    }
}
