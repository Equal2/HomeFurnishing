using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class StockHeaderSettings : EntityBase, IHistoryLog
    {
        public StockHeaderSettings()
        {
            isPostedInStock = true;
        }

        [Key]
        public int StockHeaderSettingsId { get; set; }

        [ForeignKey("DocType"), Display(Name = "Order Type")]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        public int SiteId { get; set; }
        public virtual Site Site { get; set; }
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }
        public bool? isVisibleMachine { get; set; }
        public bool? isMandatoryMachine { get; set; }
        public bool? isVisibleHeaderCostCenter { get; set; }
        public bool? isMandatoryHeaderCostCenter { get; set; }
        public bool? isVisibleLineCostCenter { get; set; }
        public bool? isMandatoryLineCostCenter { get; set; }
        public bool? isVisibleProductUID { get; set; }
        public bool? isVisibleProductCode { get; set; }
        public bool? isVisibleDimension1 { get; set; }
        public bool? isVisibleDimension2 { get; set; }
        public bool? isVisibleDimension3 { get; set; }
        public bool? isVisibleDimension4 { get; set; }
        public bool? isVisibleRate { get; set; }
        public bool? isVisibleSpecification { get; set; }
        public bool? isMandatoryRate { get; set; }
        public bool? isEditableRate { get; set; }
        public bool? isVisibleLotNo { get; set; }
        public bool? isMandatoryProcessLine { get; set; }
        public bool? isVisibleProcessLine { get; set; }
        public bool? isVisibleProcessHeader { get; set; }


        public bool isPostedInStock { get; set; }

        public bool? isPostedInStockProcess { get; set; }
        public bool? isPostedInLedger { get; set; }
        public bool? isProductHelpFromStockProcess { get; set; }
        public bool? isVisibleReferenceDocId { get; set; }

        public bool? isMandatoryProductUID { get; set; }



        public int? AdjLedgerAccountId { get; set; }

        public bool? isVisibleMaterialRequest { get; set; }
        public bool? isVisibleStockIn { get; set; }
        public bool? IsMandatoryStockIn { get; set; }

        [MaxLength(50)]
        public string PersonFieldHeading { get; set; }

        [MaxLength(100)]
        public string SqlProcDocumentPrint { get; set; }

        [MaxLength(100)]
        public string SqlFuncCurrentStock { get; set; }

        [MaxLength(100)]
        public string SqlProcDocumentPrint_AfterApprove { get; set; }
        [MaxLength(100)]
        public string SqlProcDocumentPrint_AfterSubmit { get; set; }

        [MaxLength(100)]
        public string SqlProcGatePass { get; set; }

        [MaxLength(100)]
        public string SqlProcStockProcessPost { get; set; }

        [MaxLength(100)]
        public string SqlProcStockProcessBalance { get; set; }

        [MaxLength(100)]
        public string SqlProcHelpListReferenceDocId { get; set; }
        [MaxLength(100)]
        public string SqlProcProductUidHelpList { get; set; }


        public string filterProductTypes { get; set; }
        public string filterContraSites { get; set; }
        public string filterContraDivisions { get; set; }
        public string filterProductGroups { get; set; }
        public string filterProducts { get; set; }
        public string filterContraProductDivisions { get; set; }
        public string filterContraDocTypes { get; set; }
        public string filterPersonRoles { get; set; }

        public bool? isVisibleWeight { get; set; }
        public string WeightCaption { get; set; }

        public int? LineRoundOff { get; set; }

        [ForeignKey("Process")]
        public int? ProcessId { get; set; }
        public virtual Process Process { get; set; }


        [ForeignKey("DocumentPrintReportHeader")]
        public int? DocumentPrintReportHeaderId { get; set; }
        public virtual ReportHeader DocumentPrintReportHeader { get; set; }


        [ForeignKey("OnSubmitMenu")]
        [Display(Name = "OnSubmitMenu")]
        public int? OnSubmitMenuId { get; set; }
        public virtual Menu OnSubmitMenu { get; set; }

        [ForeignKey("ImportMenu")]
        [Display(Name = "ImportMenu")]
        public int? ImportMenuId { get; set; }
        public virtual Menu ImportMenu { get; set; }

        [ForeignKey("ExportMenu")]
        [Display(Name = "ExportMenu")]
        public int? ExportMenuId { get; set; }
        public virtual Menu ExportMenu { get; set; }

        [MaxLength(20)]
        public string BarcodeStatusUpdate { get; set; }

        public int? NoOfPrintCopies { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }


    }
}
