using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.ViewModels
{
    public class StockViewModel 
    {
        [Key]
        public int StockId { get; set; }

        public int StockHeaderId { get; set; }
        public int? DocHeaderId { get; set; }

        public int? DocLineId { get; set; }

        [Display(Name = "Doc Type"), Required]
        [ForeignKey("DocType")]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }
        public string DocTypeName { get; set; }

        [Display(Name = "Doc Date"), Required]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StockHeaderDocDate { get; set; }

        [Display(Name = "Stock Doc Date"), Required]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StockDocDate { get; set; }

        public string sDocDate { get; set; }

        [Display(Name = "Doc No"), Required, MaxLength(20)]
        public string DocNo { get; set; }

        [Display(Name = "Division"), Required]
        [ForeignKey("Division")]
        public int DivisionId { get; set; }
        public virtual Division Division { get; set; }

        [ForeignKey("Site"), Display(Name = "Site")]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [Display(Name = "Currency")]
        [ForeignKey("Currency")]
        public int? CurrencyId { get; set; }
        public virtual Currency Currency { get; set; }

        [Display(Name = "HeaderProcess")]
        [ForeignKey("HeaderProcess")]
        public int? HeaderProcessId { get; set; }
        public virtual Process HeaderProcess { get; set; }

        [Display(Name = "PersonId")]
        [ForeignKey("Person")]
        public int? PersonId { get; set; }
        public virtual Person Person { get; set; }
        public string PersonName { get; set; }

        [Display(Name = "Product"), Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }


        [Display(Name = "From Godown")]
        [ForeignKey("HeaderFromGodown")]
        public int? HeaderFromGodownId { get; set; }
        public virtual Godown HeaderFromGodown { get; set; }

        [Display(Name = "Godown"), Required]
        [ForeignKey("HeaderGodown")]
        public int? HeaderGodownId { get; set; }
        public virtual Godown HeaderGodown { get; set; }

        [Display(Name = "Godown"), Required]
        [ForeignKey("Godown")]
        public int GodownId { get; set; }
        public virtual Godown Godown { get; set; }
        public string GodownName { get; set; }

        [Display(Name = "Process")]
        [ForeignKey("Process")]
        public int? ProcessId { get; set; }
        public virtual Process Process { get; set; }
        public string ProcessName { get; set; }

        [Display(Name = "Lot No."), MaxLength(10)]
        public string LotNo { get; set; }

        [ForeignKey("CostCenter"), Display(Name = "Cost Center")]
        public int? CostCenterId { get; set; }
        public virtual CostCenter CostCenter { get; set; }

        [Display(Name = "Qty_Iss")]
        public Decimal Qty_Iss { get; set; }

        [Display(Name = "Qty_Rec")]
        public Decimal Qty_Rec { get; set; }

        [Display(Name = "Rate")]
        public Decimal? Rate { get; set; }

        [Display(Name = "Weight_Iss")]
        public Decimal? Weight_Iss { get; set; }

        [Display(Name = "Weight_Rec")]
        public Decimal? Weight_Rec { get; set; }

        [Display(Name = "Expiry Date"), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime? ExpiryDate { get; set; }

        [Display(Name = "Specification")]
        public string Specification { get; set; }

        [Display(Name = "StockStatus")]
        public string StockStatus { get; set; }



        [ForeignKey("Dimension1"), Display(Name = "Dimension1")]
        public int? Dimension1Id { get; set; }
        public virtual Dimension1 Dimension1 { get; set; }

        [ForeignKey("Dimension2"), Display(Name = "Dimension2")]
        public int? Dimension2Id { get; set; }
        public virtual Dimension2 Dimension2 { get; set; }

        [ForeignKey("Dimension3"), Display(Name = "Dimension3")]
        public int? Dimension3Id { get; set; }
        public virtual Dimension3 Dimension3 { get; set; }

        [ForeignKey("Dimension4"), Display(Name = "Dimension4")]
        public int? Dimension4Id { get; set; }
        public virtual Dimension4 Dimension4 { get; set; }

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

        [ForeignKey("ReferenceDocType"), Display(Name = "ReferenceDocType")]
        public int? ReferenceDocTypeId { get; set; }
        public virtual DocumentType ReferenceDocType { get; set; }

        public int? ReferenceDocId { get; set; }


        public bool IsPostedInStockProcess { get; set; }
        public int? ProductUidId { get; set; }        
    }
}
