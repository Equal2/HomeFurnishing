using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class PackingHeader : EntityBase, IHistoryLog
    {
        public PackingHeader()
        {
            PackingLines = new List<PackingLine>();
        }

        [Key]
        public int PackingHeaderId { get; set; }
                        
        [Display(Name = "Packing Type"),Required]
        [ForeignKey("DocType")]
        [Index("IX_PackingHeader_DocID", IsUnique = true, Order = 1)]
        public int DocTypeId { get; set; }
        public virtual DocumentType DocType { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}")]                
        [Display(Name = "Packing Date"),Required ]
        public DateTime DocDate { get; set; }
        
        [Display(Name = "Packing No"),Required,MaxLength(20) ]
        [Index("IX_PackingHeader_DocID", IsUnique = true, Order = 2)]
        public string DocNo { get; set; }

        [Display(Name = "Division"),Required ]
        [ForeignKey("Division")]
        [Index("IX_PackingHeader_DocID", IsUnique = true, Order = 3)]
        public int DivisionId { get; set; }
        public virtual  Division  Division { get; set; }

        [Display(Name = "Site"), Required]
        [ForeignKey("Site")]
        [Index("IX_PackingHeader_DocID", IsUnique = true, Order = 4)]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        [ForeignKey("StockHeader")]
        [Display(Name = "Stock Header No.")]
        public int? StockHeaderId { get; set; }
        public virtual StockHeader StockHeader { get; set; }

        [ForeignKey("Buyer"), Display(Name = "Buyer"), Required]
        public int BuyerId { get; set; }
        public virtual Person Buyer { get; set; }

        [ForeignKey("JobWorker"), Display(Name = "Job Worker")]
        public int ? JobWorkerId { get; set; }
        public virtual JobWorker JobWorker { get; set; }

        [ForeignKey("Godown"), Display(Name = "Godown"), Required]
        public int GodownId { get; set; }
        public virtual Godown Godown { get; set; }

        [ForeignKey("ShipMethod"), Display(Name = "Ship Method"), Required]
        public int ShipMethodId { get; set; }
        public virtual ShipMethod ShipMethod { get; set; }


        [ForeignKey("DealUnit"), Display(Name = "DealUnit")]
        public string DealUnitId { get; set; }
        public virtual Unit DealUnit { get; set; }

        [Display(Name = "Bale No Pattern")]
        public Byte BaleNoPattern { get; set; }

        [Display(Name = "Remark")]
        public string Remark { get; set; }

        [Display(Name = "Lock Reason")]
        public string LockReason { get; set; }

        public int Status { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        public ICollection<PackingLine> PackingLines { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }

    }
}
