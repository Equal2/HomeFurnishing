using Jobs.Constants.ProductNature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jobs.Rug.Constants
{
    public static class ProductTypeConstants
    {
        public static class Rug
        {
            public const int ProductTypeId = 10001;
            public const string ProductTypeName = "Rug";
            public const int ProductNatureId = ProductNatureConstants.FinishedMaterial.ProductNatureId;

        }
        public static class Yarn
        {
            public const int ProductTypeId = 10002;
            public const string ProductTypeName = "Yarn";
            public const int ProductNatureId = ProductNatureConstants.RawMaterial.ProductNatureId;
        }
        public static class Wool
        {
            public const int ProductTypeId = 10003;
            public const string ProductTypeName = "Wool";
            public const int ProductNatureId = ProductNatureConstants.RawMaterial.ProductNatureId;
        }
        public static class Trace
        {
            public const int ProductTypeId = 10004;
            public const string ProductTypeName = "Trace";
            public const int ProductNatureId = ProductNatureConstants.RawMaterial.ProductNatureId;
        }
        public static class Map
        {
            public const int ProductTypeId = 10005;
            public const string ProductTypeName = "Map";
            public const int ProductNatureId = ProductNatureConstants.RawMaterial.ProductNatureId;
        }
    }
}