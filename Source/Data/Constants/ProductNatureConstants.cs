using Jobs.Constants.LedgerAccount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jobs.Constants.ProductNature
{
    public static class ProductNatureConstants
    {
        public static class RawMaterial
        {
            public const int ProductNatureId = 1;
            public const string ProductNatureName = "Raw Material";
        }
        public static class FinishedMaterial
        {
            public const int ProductNatureId = 2;
            public const string ProductNatureName = "Finished Material";
        }
        public static class OtherMaterial
        {
            public const int ProductNatureId = 3;
            public const string ProductNatureName = "Other Material";
        }
         public static class Bom
        {
            public const int ProductNatureId = 4;
            public const string ProductNatureName = "Bom";
        }
        public static class Form
        {
            public const int ProductNatureId = 5;
            public const string ProductNatureName = "Form";
        }
        public static class Machine
        {
            public const int ProductNatureId = 6;
            public const string ProductNatureName = "Machine";
        }
        public static class LedgerAccount
        {
            public const int ProductNatureId = 7;
            public const string ProductNatureName = "Ledger Account";
        }
    }
}