using Jobs.Constants.City;
using Jobs.Constants.TdsCategory;
using Jobs.Constants.TdsGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jobs.Constants.TdsRate
{
    public static class TdsRateConstants
    {
        public static class ContractorCompanyFirm
        {
            public const int TdsRateId = 1;
            public const int TdsCategoryId = TdsCategoryConstants.Contractor.TdsCategoryId;
            public const int TdsGroupId = TdsGroupConstants.CompanyFirm.TdsGroupId;
            public const double Percentage = 2;
        }
        public static class ContractorIndividual
        {
            public const int TdsRateId = 2;
            public const int TdsCategoryId = TdsCategoryConstants.Contractor.TdsCategoryId;
            public const int TdsGroupId = TdsGroupConstants.Individual.TdsGroupId;
            public const double Percentage = 1;
        }
        public static class ContractorInvalidPan
        {
            public const int TdsRateId = 3;
            public const int TdsCategoryId = TdsCategoryConstants.Contractor.TdsCategoryId;
            public const int TdsGroupId = TdsGroupConstants.InvalidPan.TdsGroupId;
            public const double Percentage = 20;
        }
    }
}