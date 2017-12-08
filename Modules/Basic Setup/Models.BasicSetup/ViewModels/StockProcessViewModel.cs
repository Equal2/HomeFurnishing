using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Models.Company.Models;

namespace Models.BasicSetup.ViewModels
{
    public class StockProcessViewModel
    {
        [Key]
        public int StockProcessId { get; set; }

        public int StockHeaderId { get; set; }
        public int? DocHeaderId { get; set; }

        public int? DocLineId { get; set; }

        [Display(Name = "Doc Type"), Required]
        [ForeignKey("DocType")]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [Display(Name = "Doc Date"), Required]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StockHeaderDocDate { get; set; }

        [Display(Name = "StockProcess Doc Date"), Required]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StockProcessDocDate { get; set; }

        [Display(Name = "Doc No"), Required, MaxLength(20)]
        public string DocNo { get; set; }

        [Display(Name = "Division"), Required]
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }

        [Display(Name = "Site")]
        public int SiteId { get; set; }
        public string SiteName { get; set; }

        [Display(Name = "Currency")]
        public int? CurrencyId { get; set; }
        public string CurrencyName { get; set; }

        [Display(Name = "HeaderProcess")]
        public int? HeaderProcessId { get; set; }
        public string HeaderProcessName { get; set; }

        [Display(Name = "PersonId")]
        public int? PersonId { get; set; }
        public string PersonName { get; set; }

        [Display(Name = "Product"), Required]
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        [Display(Name = "From Godown")]
        public int? HeaderFromGodownId { get; set; }
        public string HeaderFromGodownName { get; set; }

        [Display(Name = "Godown"), Required]
        public int? HeaderGodownId { get; set; }
        public string HeaderGodownName { get; set; }

        [Display(Name = "Godown"), Required]
        public int ? GodownId { get; set; }
        public string GodownName { get; set; }

        [Display(Name = "Process")]
        public int? ProcessId { get; set; }
        public string ProcessName { get; set; }

        [Display(Name = "Lot No."), MaxLength(10)]
        public string LotNo { get; set; }

        [Display(Name = "Cost Center")]
        public int? CostCenterId { get; set; }
        public string CostCenterName { get; set; }

        [Display(Name = "Qty_Iss")]
        public Decimal Qty_Iss { get; set; }

        [Display(Name = "Qty_Rec")]
        public Decimal Qty_Rec { get; set; }

        [Display(Name = "Rate")]
        public Decimal? Rate { get; set; }

        [Display(Name = "Expiry Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime? ExpiryDate { get; set; }

        [Display(Name = "Specification")]
        public string Specification { get; set; }

        [Display(Name = "Dimension1")]
        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }

        [Display(Name = "Dimension2")]
        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }
        public string HeaderRemark { get; set; }
        public int Status { get; set; }

        public int? StockHeaderExist { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        public bool IsPostedInStockProcess { get; set; }
        public int? ProductUidId { get; set; }
   
    }
}
