using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jobs.Constants.ChargeGroupPerson
{
    public static class ChargeGroupPersonConstants
    {
        public static class StateRegistered
        {
            public const int ChargeGroupPersonId = 1;
            public const string ChargeGroupPersonName = "State (Registered)";
        }
        public static class StateUnRegistered
        {
            public const int ChargeGroupPersonId = 2;
            public const string ChargeGroupPersonName = "State (UnRegistered)";
        }
        public static class StateComposition
        {
            public const int ChargeGroupPersonId = 3;
            public const string ChargeGroupPersonName = "State (Composition)";
        }
        public static class ExStateRegistered
        {
            public const int ChargeGroupPersonId = 4;
            public const string ChargeGroupPersonName = "Ex-State (Registered)";
        }
        public static class ExStateUnRegistered
        {
            public const int ChargeGroupPersonId = 5;
            public const string ChargeGroupPersonName = "Ex-State (UnRegistered)";
        }
    }
}