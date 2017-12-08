using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class SaleDispatchReturnHeader : EntityBase, IHistoryLog
    {


        [Key]        
        public int SaleDispatchReturnHeaderId { get; set; }
                        
        [Display(Name = "Return Type"),Required]
        [ForeignKey("DocType")]
        [Index("IX_SaleDispatchReturnHeader_DocID", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Return Date"),Required ]
        public DateTime  DocDate { get; set; }
        
        [Display(Name = "Return No"),Required,MaxLength(20,ErrorMessage = "{0} can not exceed {1} characters")]
        [Index("IX_SaleDispatchReturnHeader_DocID", IsUnique = true, Order = 2)]
        public string DocNo { get; set; }

        [ForeignKey("Reason"), Display(Name = "Reason")]
        public int ReasonId { get; set; }
        public virtual Reason Reason { get; set; }

        [Display(Name = "Division"),Required ]
        [ForeignKey("Division")]
        [Index("IX_SaleDispatchReturnHeader_DocID", IsUnique = true, Order = 3)]
        public int DivisionId { get; set; }
        public virtual  Division  Division { get; set; }

        [Display(Name = "Site"), Required]
        [ForeignKey("Site")]
        [Index("IX_SaleDispatchReturnHeader_DocID", IsUnique = true, Order = 4)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [ForeignKey("Buyer")]
        [Display(Name = "Buyer Name")]
        public int BuyerId { get; set; }
        public virtual Buyer Buyer { get; set; }

        [Display(Name = "Remark"),Required]
        public string Remark { get; set; }

        [Display(Name = "Status")]
        public int Status { get; set; }

        [ForeignKey("StockHeader")]
        [Display(Name = "Stock Header No.")]
        public int? StockHeaderId { get; set; }
        public virtual StockHeader StockHeader { get; set; }


        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

        [Display(Name = "Lock Reason Delete")]
        public string LockReasonDelete { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [ForeignKey("Godown")]
        public int GodownId { get; set; }
        public virtual Godown Godown { get; set; }

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
