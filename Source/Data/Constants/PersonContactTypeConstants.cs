using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jobs.Constants.PersonContactType
{
    public static class PersonContactTypeConstants
    {
        public static class Office
        {
            public const int PersonContactTypeId = 1;
            public const string PersonContactTypeName = "Office";
        }
        public static class Work
        {
            public const int PersonContactTypeId = 2;
            public const string PersonContactTypeName = "Work";
        }
        public static class Residence
        {
            public const int PersonContactTypeId = 3;
            public const string PersonContactTypeName = "Residence";
        }
    }
}