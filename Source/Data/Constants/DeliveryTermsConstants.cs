using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jobs.Constants.DeliveryTerms
{
    public static class DeliveryTermsConstants
    {
        public static class FOB
        {
            public const int DeliveryTermsId = 1;
            public const string DeliveryTermsName = "F.O.B.";
            public const string PrintingDescription = "F.O.B.";
        }
        public static class CIF
        {
            public const int DeliveryTermsId = 2;
            public const string DeliveryTermsName = "C.I.F.";
            public const string PrintingDescription = "C.I.F.";
        }
    }
}