using Model;
using Models.BasicSetup.Models;
using Models.Company.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Customize.Models
{
    public class JobReceiveSettings : EntityBase, IHistoryLog
    {

        [Key]
        public int JobReceiveSettingsId { get; set; }

        [ForeignKey("DocType"), Display(Name = "Order Type")]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }      

        public int SiteId { get; set; }
        public virtual Site Site { get; set; }
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }
        public bool? isVisibleMachine { get; set; }
        public bool? isMandatoryMachine { get; set; }
        public bool? isVisibleProductUID { get; set; }
        public bool? isVisibleDimension1 { get; set; }
        public bool? isVisibleDimension2 { get; set; }        
        public bool? isVisibleLotNo { get; set; }
        public bool? isVisibleLoss { get; set; }
        public bool ? IsVisibleWeight { get; set; }
        public bool ? IsMandatoryWeight { get; set; }
        public bool? IsVisibleForOrderMultiple { get; set; }
        public bool? isPostedInStock { get; set; }
        public bool? isPostedInStockProcess { get; set; }
        public bool? isPostedInStockVirtual { get; set; }

        public string filterContraSites { get; set; }
        public string filterContraDivisions { get; set; }


        [MaxLength(100)]
        public string SqlProcGatePass { get; set; }



        /// <summary>
        /// DocId will be passed as a parameter in specified procedure.
        /// Procedure should have only one parameter of type int.
        /// </summary>
        [MaxLength(100)]
        public string SqlProcDocumentPrint { get; set; }

        /// <summary>
        /// DocId will be passed as a parameter in specified procedure.
        /// Procedure should have only one parameter of type int.
        /// </summary>
        [MaxLength(100)]
        public string SqlProcDocumentPrint_AfterSubmit { get; set; }

        /// <summary>
        /// DocId will be passed as a parameter in specified procedure.
        /// Procedure should have only one parameter of type int.
        /// </summary>
        [MaxLength(100)]
        public string SqlProcDocumentPrint_AfterApprove { get; set; }

        [MaxLength(100)]
        public string DocumentPrint { get; set; }
        
        [MaxLength (100)]
        public string SqlProcConsumption { get; set; }


        public string SqlProcGenProductUID { get; set; }
        public string filterLedgerAccountGroups { get; set; }
        public string filterLedgerAccounts { get; set; }
       
        public string filterProductTypes { get; set; }
        public string filterProductGroups { get; set; }
        public string filterProducts { get; set; }
        public string filterContraDocTypes { get; set; }

        [ForeignKey("Process")]
        public int ProcessId { get; set; }
        public virtual Process Process { get; set; }

        [Display(Name = "ImportMenu")]
        public int? ImportMenuId { get; set; }


        [Display(Name = "WizardMenu")]
        public int? WizardMenuId { get; set; }

        [Display(Name = "OnSubmitMenu")]
        public int? OnSubmitMenuId { get; set; }


        [Display(Name = "OnApproveMenu")]
        public int? OnApproveMenuId { get; set; }


        [ForeignKey("Calculation")]
        public int ? CalculationId { get; set; }
        public virtual Calculation Calculation { get; set; }

        [MaxLength(20)]
        public string BarcodeStatusUpdate { get; set; }

        [MaxLength(20)]
        public string StockQty { get; set; }


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
