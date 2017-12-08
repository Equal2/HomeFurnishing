using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jobs.Constants.ShipMethod
{
    public static class ShipMethodConstants
    {
        public static class ByRoad
        {
            public const int ShipMethodId = 1;
            public const string ShipMethodName = "By Road";
        }
        public static class BySea
        {
            public const int ShipMethodId = 2;
            public const string ShipMethodName = "By Sea";
        }
        public static class ByAir
        {
            public const int ShipMethodId = 3;
            public const string ShipMethodName = "By Air";
        }
    }
}