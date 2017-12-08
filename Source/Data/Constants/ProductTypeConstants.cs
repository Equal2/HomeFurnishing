using Jobs.Constants.LedgerAccount;
using Jobs.Constants.ProductNature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jobs.Constants.ProductType
{
    public static class ProductTypeConstants
    {
        public static class RawMaterial
        {
            public const int ProductTypeId = 1;
            public const string ProductTypeName = "Raw Material";
            public const int ProductNatureId = ProductNatureConstants.RawMaterial.ProductNatureId;
        }
        public static class FinishedMaterial
        {
            public const int ProductTypeId = 2;
            public const string ProductTypeName = "Finished Material";
            public const int ProductNatureId = ProductNatureConstants.FinishedMaterial.ProductNatureId;
        }
        public static class OtherMaterial
        {
            public const int ProductTypeId = 3;
            public const string ProductTypeName = "Other Material";
            public const int ProductNatureId = ProductNatureConstants.OtherMaterial.ProductNatureId;
        }
         public static class Bom
        {
            public const int ProductTypeId = 4;
            public const string ProductTypeName = "Bom";
            public const int ProductNatureId = ProductNatureConstants.Bom.ProductNatureId;
        }
        public static class Form
        {
            public const int ProductTypeId = 5;
            public const string ProductTypeName = "Form";
            public const int ProductNatureId = ProductNatureConstants.Form.ProductNatureId;
        }
        public static class Machine
        {
            public const int ProductTypeId = 6;
            public const string ProductTypeName = "Machine";
            public const int ProductNatureId = ProductNatureConstants.Machine.ProductNatureId;
        }
        public static class LedgerAccount
        {
            public const int ProductTypeId = 7;
            public const string ProductTypeName = "Ledger Account";
            public const int ProductNatureId = ProductNatureConstants.LedgerAccount.ProductNatureId;
        }
    }
}