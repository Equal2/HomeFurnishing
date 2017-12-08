using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class CarpetSkuSettings : EntityBase, IHistoryLog
    {
        public CarpetSkuSettings()
        {
        }

        [Key]
        public int CarpetSkuSettingsId { get; set; }
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }


        public bool? isVisibleProductDesign { get; set; }
        public bool? isVisibleProductStyle { get; set; }
        public bool? isVisibleProductManufacturer { get; set; }
        public bool? isVisibleProductDesignPattern { get; set; }
        public bool? isVisibleContent { get; set; }
        public bool? isVisibleOriginCountry { get; set; }
        public bool? isVisibleInvoiceGroup { get; set; }
        public bool? isVisibleDrawbackTarrif { get; set; }
        public bool? isVisibleStandardCost { get; set; }
        public bool? isVisibleStandardWeight { get; set; }
        public bool? isVisibleGrossWeight { get; set; }
        public bool? isVisibleSupplierDetail { get; set; }
        public bool? isVisibleSample { get; set; }
        public bool? isVisibleCounterNo { get; set; }
        public bool? isVisibleTags { get; set; }
        public bool? isVisibleDivision { get; set; }
        public bool? isVisibleColour { get; set; }
        public bool? isVisibleProductionRemark { get; set; }
        public bool? isVisibleMapScale { get; set; }
        public bool? isVisibleCBM { get; set; }
        public bool? isVisibleTraceType { get; set; }
        public bool? isVisibleMapType { get; set; }
        public bool? isVisibleStencilSize { get; set; }
        public bool? isVisibleSalesTaxProductCode { get; set; }




        [ForeignKey("ProductDesign")]
        public int? ProductDesignId { get; set; }
        public virtual ProductDesign ProductDesign { get; set; }

        [ForeignKey("OriginCountry")]
        public int? OriginCountryId { get; set; }
        public virtual Country OriginCountry { get; set; }


        [ForeignKey("PerimeterSizeType")]
        [Display(Name = "PerimeterSizeType")]
        public int? PerimeterSizeTypeId { get; set; }
        public virtual ProductSizeType PerimeterSizeType { get; set; }

        public string UnitConversions { get; set; }

        public string NameBaseOnSize { get; set; }
        
        [MaxLength(50)]
        public string SalesTaxProductCodeCaption { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
