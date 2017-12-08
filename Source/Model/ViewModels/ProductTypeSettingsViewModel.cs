using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class ProductTypeSettingsViewModel
    {
        public int ProductTypeSettingsId { get; set; }
        public int ProductTypeId { get; set; }
        public string ProductTypeName { get; set; }

        public string UnitId { get; set; }

        public bool isShowMappedDimension1 { get; set; }
        public bool isShowUnMappedDimension1 { get; set; }
        public bool isApplicableDimension1 { get; set; }
        public string Dimension1Caption { get; set; }


        public bool isShowMappedDimension2 { get; set; }
        public bool isShowUnMappedDimension2 { get; set; }
        public bool isApplicableDimension2 { get; set; }
        public string Dimension2Caption { get; set; }


        public bool isShowMappedDimension3 { get; set; }
        public bool isShowUnMappedDimension3 { get; set; }
        public bool isApplicableDimension3 { get; set; }
        public string Dimension3Caption { get; set; }


        public bool isShowMappedDimension4 { get; set; }
        public bool isShowUnMappedDimension4 { get; set; }
        public bool isApplicableDimension4 { get; set; }
        public string Dimension4Caption { get; set; }



        public bool isVisibleProductDescription { get; set; }
        public bool isVisibleProductSpecification { get; set; }
        public bool isVisibleProductCategory { get; set; }
        public bool isVisibleSalesTaxGroup { get; set; }
        public bool isVisibleSaleRate { get; set; }
        public bool isVisibleStandardCost { get; set; }
        public bool isVisibleTags { get; set; }
        public bool isVisibleMinimumOrderQty { get; set; }
        public bool isVisibleReOrderLevel { get; set; }
        public bool isVisibleGodownId { get; set; }
        public bool isVisibleBinLocationId { get; set; }
        public bool isVisibleProfitMargin { get; set; }
        public bool isVisibleCarryingCost { get; set; }
        public bool isVisibleLotManagement { get; set; }
        public bool isVisibleConsumptionDetail { get; set; }
        public bool isVisibleProductProcessDetail { get; set; }


        public bool isVisibleDefaultDimension1 { get; set; }
        public bool isVisibleDefaultDimension2 { get; set; }
        public bool isVisibleDefaultDimension3 { get; set; }
        public bool isVisibleDefaultDimension4 { get; set; }


        public bool isVisibleDiscontinueDate { get; set; }
        public bool isVisibleSalesTaxProductCode { get; set; }

        public string IndexFilterParameter { get; set; }
        public string ProductNameCaption { get; set; }
        public string ProductCodeCaption { get; set; }
        public string ProductDescriptionCaption { get; set; }
        public string ProductSpecificationCaption { get; set; }
        public string ProductGroupCaption { get; set; }
        public string ProductCategoryCaption { get; set; }
        public string SalesTaxProductCodeCaption { get; set; }
        public string SqlProcProductCode { get; set; }

    }
}
