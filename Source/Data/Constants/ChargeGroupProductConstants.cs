using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jobs.Constants.ChargeGroupProduct
{
    public static class ChargeGroupProductConstants
    {
        public static class GST5Per
        {
            public const int ChargeGroupProductId = 1;
            public const string ChargeGroupProductName = "GST 5%";
        }
        public static class GST12Per
        {
            public const int ChargeGroupProductId = 2;
            public const string ChargeGroupProductName = "GST 12%";
        }
        public static class GST18Per
        {
            public const int ChargeGroupProductId = 3;
            public const string ChargeGroupProductName = "GST 18%";
        }
        public static class GST28Per
        {
            public const int ChargeGroupProductId = 4;
            public const string ChargeGroupProductName = "GST 28%";
        }
        public static class GSTExempt
        {
            public const int ChargeGroupProductId = 5;
            public const string ChargeGroupProductName = "GST Exempt";
        }
    }
}