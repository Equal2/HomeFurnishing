using Jobs.Constants.Company;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jobs.Constants.ChargeType
{
    public static class ChargeTypeConstants
    {
        public static class Amount
        {
            public const int ChargeTypeId = 1;
            public const string ChargeTypeName = "Amount";
        }
        public static class TaxableAmount
        {
            public const int ChargeTypeId = 2;
            public const string ChargeTypeName = "Taxable Amount";
        }
        public static class CGST
        {
            public const int ChargeTypeId = 3;
            public const string ChargeTypeName = "CGST";
        }
        public static class SGST
        {
            public const int ChargeTypeId = 4;
            public const string ChargeTypeName = "SGST";
        }
        public static class IGST
        {
            public const int ChargeTypeId = 5;
            public const string ChargeTypeName = "IGST";
        }
        public static class CESS
        {
            public const int ChargeTypeId = 6;
            public const string ChargeTypeName = "CESS";
        }
        public static class SubTotal
        {
            public const int ChargeTypeId = 7;
            public const string ChargeTypeName = "Sub Total";
        }
        public static class RoundOff
        {
            public const int ChargeTypeId = 8;
            public const string ChargeTypeName = "Round Off";
        }
    }
}